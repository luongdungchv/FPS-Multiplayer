using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;

public partial class NetworkFPSPlayer : Kigor.Networking.NetworkPlayer
{
#if SERVER_BUILD
    private FPSInputPacket pendingInputPacket;
    private int lastProcessedTick = -1;

    protected partial void Awake()
    {
        statesBuffer = new FPSPlayerState[TickScheduler.MAX_TICK];
        Debug.Log(statesBuffer[0].init);

    }
    protected partial void Update()
    {

    }
    protected void Start()
    {
        this.tickScheduler.RegisterTickCallback(this.TickUpdate);
    }
    protected partial void TickUpdate()
    {
        if (pendingInputPacket == null) return;
        lock (pendingInputPacket)
        {
            this.PerformRotation(pendingInputPacket);
            this.PerformMovement(pendingInputPacket);
            this.PerformVerticalMovement();

            if (pendingInputPacket.jump)
            {
                this.PerformJump();
            }

            this.SendReconcilePacket(pendingInputPacket.tick);
            this.WriteStateToBuffer(pendingInputPacket);

            lastProcessedTick = pendingInputPacket.tick;
            Debug.Log(this.GroundCheck(lastProcessedTick, out var pos));
            this.pendingInputPacket = null;
        }
    }

    public override void Initialize(SocketWrapper socket, NetworkGameRoom room, int id)
    {
        base.Initialize(socket, room, id);

        base.Initialize(socket, room, id);

        this.msgHandler.Add(PacketType.FPS_INPUT_PACKET, this.HandleInputPacket);
    }

    private void HandleInputPacket(byte[] data)
    {
        var packet = new FPSInputPacket();
        packet.DecodeMessage(data);
        this.pendingInputPacket = packet;

        // ThreadManager.ExecuteOnMainThread(() =>
        // {
        //     // this.PerformRotation(packet);
        //     // this.PerformMovement(packet);
        //     // this.SendReconcilePacket(packet.tick);

        //     // this.WriteStateToBuffer();
        // });

    }

    private void WriteStateToBuffer(FPSInputPacket packet)
    {
        var state = new FPSPlayerState()
        {
            position = transform.position,
            horizontalRotation = transform.eulerAngles.y,
        };

        var vertRot = this.headTransform.transform.eulerAngles.x;
        if (vertRot > 180)
            vertRot -= 360;
        state.verticalRotation = vertRot;
        state.init = true;

        this.statesBuffer[packet.tick] = state;
    }

    private void PerformMovement(FPSInputPacket packet)
    {
        var lookDir = new Vector3(packet.moveDir.x, 0, packet.moveDir.y);
        var rightDir = new Vector3(lookDir.z, 0, -lookDir.x);

        var x = packet.movement.x;
        var y = packet.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var velocity = moveDir.normalized * moveSpd;

        transform.position += velocity * this.tickScheduler.TickDeltaTime;
    }

    private void PerformRotation(FPSInputPacket packet)
    {
        var characterAngle = Mathf.Atan2(packet.moveDir.x, packet.moveDir.y) * Mathf.Rad2Deg;
        var cameraAngle = packet.cameraAngle;

        var currentRot = transform.eulerAngles;
        currentRot.y = characterAngle;
        transform.eulerAngles = currentRot;

        var currentHeadRot = headTransform.localEulerAngles;
        currentHeadRot.x = cameraAngle;
        headTransform.localEulerAngles = currentHeadRot;

    }
    private void PerformJump()
    {
        if (!inAir)
        {
            this.inAir = true;
            this.currentJump = this.jumpSpd;
        }
        pendingInputPacket.jump = false;
    }
    private void PerformVerticalMovement()
    {
        if (inAir)
        {
            this.currentJump -= gravity * this.tickScheduler.TickDeltaTime;
            var vel = Vector3.up * currentJump;
            transform.position += Vector3.up * currentJump;
            var groundCheck = this.GroundCheck(pendingInputPacket.tick, out var groundPos);
            if (currentJump < 0 && groundCheck)
            {
                this.inAir = false;
                this.currentJump = 0;
                transform.position = groundPos + Vector3.up * (this.height + 0.001f);
                Debug.Log($"Ground Check: {groundCheck}, {transform.position}");
            }
        }
    }

    private bool GroundCheck(int tick, out Vector3 groundPos)
    {
        var physicsScene = this.room.PhysicsScene;
        var currentCheck = physicsScene.Raycast(groundCheckPoint.position, Vector3.down, out var hitInfo, 0.1f, this.groundMask);
        groundPos = hitInfo.point;

        var lastTick = this.lastProcessedTick;
        if(lastTick == -1) return false;

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
        packet.playerState.verticalRotation = headTransform.localEulerAngles.x;

        packet.tick = tick;

        var data = packet.EncodeData();
        this.socket.SendDataUDP(data);
    }

#endif
}
