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
        var lookDir = new Vector3(packet.moveDir.x, 0, packet.moveDir.y);
        var rightDir = new Vector3(lookDir.z, 0, -lookDir.x);

        var x = packet.movement.x;
        var y = packet.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var velocity = moveDir.normalized * moveSpd;

        transform.position += velocity * this.tickScheduler.TickDeltaTime;
    }

    private void PerformRotation(FPSInputPacket packet){
        var characterAngle = Mathf.Atan2(packet.moveDir.x, packet.moveDir.y) * Mathf.Rad2Deg;
        var cameraAngle = packet.cameraAngle;
        
        var currentRot = transform.eulerAngles;
        currentRot.y = characterAngle;
        transform.eulerAngles = currentRot;

        var currentHeadRot = headTransform.localEulerAngles;
        currentHeadRot.x = cameraAngle;
        headTransform.localEulerAngles = currentHeadRot;

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
