using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;
using UnityEngine.Events;

public partial class NetworkFPSPlayer : Kigor.Networking.NetworkPlayer
{
#if SERVER_BUILD
    private int currentClientTick;
    private PlayerAnimationController animationController;
    public int CurrentClientTick => this.currentClientTick;

    protected partial void Awake()
    {
        statesBuffer = new FPSPlayerState[TickScheduler.MAX_TICK];
        this.animationController = this.GetComponent<PlayerAnimationController>();
        Debug.Log(statesBuffer[0].init);
    }

    protected partial void Update()
    {
        this.currentState.position = transform.position;
    }

    protected void Start()
    {
        this.TickScheduler.RegisterTickCallback(this.TickUpdate);
    }

    protected partial void TickUpdate()
    {
    }

    public override void Initialize(SocketWrapper socket, NetworkGameRoom room, int id)
    {
        base.Initialize(socket, room, id);

        this.msgHandler.Add(PacketType.FPS_INPUT_PACKET, this.HandleInputPacket);
        this.msgHandler.Add(PacketType.FPS_SHOOT, this.HandleShootPacket);
        this.msgHandler.Add(PacketType.FPS_WEAPON_RELOAD, this.HandleReloadPacket);
        this.msgHandler.Add(PacketType.FPS_WEAPON_CHANGE, this.HandleWeaponChangePacket);

        this.WeaponController.ChangeWeapon(WeaponEnum.AK47);
    }

    #region COMMAND_HANDLING

    private void HandleInputPacket(byte[] data)
    {
        var packet = new FPSInputPacket();
        packet.DecodeMessage(data);
        int tickOffset = packet.tick - this.currentClientTick;
        if (tickOffset < 0) tickOffset += TickScheduler.MAX_TICK;
        this.currentClientTick = packet.tick;
        ThreadManager.ExecuteOnMainThread(() =>
        {
            this.Controller.PerformRotation(packet);

            currentState.position = transform.position;
            currentState.horizontalRotation = transform.eulerAngles.y;

            this.Controller.PerformMovement(packet);

            if (packet.jump)
            {
                this.Controller.PerformJump(packet);
            }

            this.animationController.UpdateAnimation(tickOffset * this.TickScheduler.TickDeltaTime);

            this.SendReconcilePacket(packet.tick);
            this.WriteStateToBuffer(packet);

            lastTick = packet.tick;
        }, ExecuteFunction.Update);
    }

    private void HandleShootPacket(byte[] data)
    {
        var packet = new FPSShootPacket();
        packet.DecodeMessage(data);
        this.Room.BroadcastMessage(packet.EncodeData());
        ThreadManager.ExecuteOnMainThread(() => this.WeaponController.HandleShootPacket(packet));
    }

    private void HandleReloadPacket(byte[] data)
    {
        var packet = new FPSWeaponReloadPacket();
        packet.DecodeMessage(data);
        ThreadManager.ExecuteOnMainThread(() => this.WeaponController.HandleReloadPacket(packet));
    }

    private void HandleWeaponChangePacket(byte[] data)
    {
        var packet = new FPSWeaponChangePacket();
        packet.DecodeMessage(data);
        ThreadManager.ExecuteOnMainThread(() => this.WeaponController.ChangeWeapon(packet.weapon));
        this.Room.BroadcastMessage(packet.EncodeData());
    }

    public void RevertState(int tickCount)
    {
        var targetTick = this.currentClientTick - tickCount;
        if (targetTick < 0) targetTick = TickScheduler.MAX_TICK + targetTick;
        var targetState = this.statesBuffer[targetTick];
        transform.position = targetState.position;
    }

    public void RestoreState()
    {
        this.transform.position = this.Position;
    }

    #endregion

    private void WriteStateToBuffer(FPSInputPacket packet)
    {
        var state = new FPSPlayerState()
        {
            position = transform.position,
            horizontalRotation = transform.eulerAngles.y,
        };

        var vertRot = this.Avatar.HeadTransform.transform.eulerAngles.x;
        if (vertRot > 180)
            vertRot -= 360;
        state.verticalRotation = vertRot;
        state.init = true;
        
        this.Controller.HandleAnimation(packet, ref state);

        this.currentState = state;

        this.statesBuffer[packet.tick] = state;
    }

    private void SendReconcilePacket(byte tick)
    {
        var packet = new FPSReconcilePacket();

        packet.playerState.position = transform.position;
        packet.playerState.horizontalRotation = transform.eulerAngles.y;
        packet.playerState.verticalRotation = Avatar.HeadTransform.localEulerAngles.x;

        packet.tick = tick;

        var data = packet.EncodeData();
        this.socket.SendDataUDP(data);
    }

#endif
}

public partial struct FPSPlayerState
{
#if SERVER_BUILD
    public float animStateTime;
    public int animStateIndex;
#endif
}