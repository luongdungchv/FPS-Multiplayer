using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;
using UnityEngine.Events;

public partial class NetworkFPSPlayer : Kigor.Networking.NetworkPlayer
{
#if SERVER_BUILD
    private FPSInputPacket pendingInputPacket;
    private Queue<UnityAction> pendingTickUpdate;
    private int lastProcessedTick = -1;

    protected partial void Awake()
    {
        statesBuffer = new FPSPlayerState[TickScheduler.MAX_TICK];
        this.pendingTickUpdate = new Queue<UnityAction>();
        Debug.Log(statesBuffer[0].init);

    }
    protected partial void Update()
    {

    }
    protected void Start()
    {
        this.TickScheduler.RegisterTickCallback(this.TickUpdate);
    }
    protected partial void TickUpdate()
    {
        // if (pendingInputPacket == null) return;
        // lock (pendingInputPacket)
        // {
        //     this.Controller.PerformRotation(pendingInputPacket);
        //     this.Controller.PerformMovement(pendingInputPacket);
        //     this.Controller.PerformVerticalMovement(pendingInputPacket);

        //     if (pendingInputPacket.jump)
        //     {
        //         this.Controller.PerformJump(pendingInputPacket);
        //     }

        //     this.SendReconcilePacket(pendingInputPacket.tick);
        //     this.WriteStateToBuffer(pendingInputPacket);

        //     lastProcessedTick = pendingInputPacket.tick;
        //     this.pendingInputPacket = null;
        // }
        if (pendingTickUpdate.Count > 0)
        {
            pendingTickUpdate.Dequeue().Invoke();
        }
    }

    public override void Initialize(SocketWrapper socket, NetworkGameRoom room, int id)
    {
        base.Initialize(socket, room, id);

        this.msgHandler.Add(PacketType.FPS_INPUT_PACKET, this.HandleInputPacket);
    }

    private void HandleInputPacket(byte[] data)
    {
        var packet = new FPSInputPacket();
        packet.DecodeMessage(data);
        this.pendingInputPacket = packet;

        ThreadManager.ExecuteOnMainThread(() =>
        {
            this.Controller.PerformRotation(packet);
            this.Controller.PerformMovement(packet);
            this.Controller.PerformVerticalMovement(packet);

            if (packet.jump)
            {
                this.Controller.PerformJump(packet);
            }

            this.SendReconcilePacket(packet.tick);
            this.WriteStateToBuffer(packet);

            Debug.Log((packet.tick, this.statesBuffer[packet.tick].position));

            lastProcessedTick = packet.tick;
        });
        // this.pendingTickUpdate.Enqueue(() =>
        // {
        //     this.Controller.PerformRotation(packet);
        //     this.Controller.PerformMovement(packet);
        //     this.Controller.PerformVerticalMovement(packet);

        //     if (packet.jump)
        //     {
        //         this.Controller.PerformJump(packet);
        //     }

        //     this.SendReconcilePacket(packet.tick);
        //     this.WriteStateToBuffer(packet);

        //     lastProcessedTick = packet.tick;
        // });

    }

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

        this.statesBuffer[packet.tick] = state;
    }



    public bool GroundCheck(int tick, out Vector3 groundPos)
    {
        var physicsScene = this.room.PhysicsScene;
        var currentCheck = physicsScene.Raycast(groundCheckPoint.position, Vector3.down, out var hitInfo, 0.1f, this.groundMask);
        groundPos = hitInfo.point;

        var lastTick = this.lastProcessedTick;
        if (lastTick == -1) return false;

        var lastState = this.statesBuffer[lastTick];
        if (lastState.init)
        {
            var lastGroundPos = lastState.position + VectorUtils.Multiply(groundCheckPoint.localPosition, transform.localScale);
            var check = physicsScene.Raycast(lastGroundPos, groundCheckPoint.position - lastGroundPos, out hitInfo, (groundCheckPoint.position - lastGroundPos).magnitude + 0.1f, this.groundMask);
            //Debug.Log((groundCheckPoint.position.y, lastGroundPos.y, check, tick, lastTick));
            if (check)
            {
                currentCheck = currentCheck || check;
                groundPos = hitInfo.point;
            }
        }
        return currentCheck;
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
