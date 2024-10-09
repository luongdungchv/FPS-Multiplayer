using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;

public partial class NetworkFPSPlayer : Kigor.Networking.NetworkPlayer
{
#if SERVER_BUILD

    protected partial void Awake(){


    }
    protected partial void Update(){

    }
    protected partial void TickUpdate(){

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
        Debug.Log(packet.movement);

        ThreadManager.ExecuteOnMainThread(() => {
            this.PerformRotation(packet);
            this.PerformMovement(packet);
            this.SendReconcilePacket(packet.tick);
        });

    }

    private void PerformMovement(FPSInputPacket packet)
    {
        var lookDir = transform.forward;
        var rightDir = transform.right;

        var x = packet.movement.x;
        var y = packet.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var velocity = moveDir.normalized * moveSpd;

        transform.position += velocity * this.tickScheduler.TickDeltaTime;
    }

    private void PerformRotation(FPSInputPacket packet){
        var mouseX = packet.mouseX;
        var mouseY = packet.mouseY;
        
        var currentRot = transform.eulerAngles;
        currentRot.y += mouseX * this.mouseSen;

        this.PerformHeadRotation(mouseY, mouseSen);

        transform.eulerAngles = currentRot;
    }
    private void PerformHeadRotation(float amount, float sensitivity){
        var currentRot = headTransform.localEulerAngles;
        var mouseY = amount * sensitivity;
        Debug.Log(mouseY);

        if(currentRot.x > 180)
            currentRot.x -= 360;
        currentRot.x -= mouseY;
        currentRot.x = Mathf.Clamp(currentRot.x, -90f, 90f);

        headTransform.localEulerAngles = currentRot;
    }

    private void SendReconcilePacket(byte tick){
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
