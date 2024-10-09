using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;
using System.Linq;

public partial class FPSPlayer : Kigor.Networking.NetworkPlayer
{
    #if SERVER_BUILD
    private float deltaTime => this.room.Rule.TickScheduler.TickDeltaTime;
    protected void TickUpdate()
    {
        //Debug.Log("test");
    }

    public override void Initialize(SocketWrapper socket, NetworkGameRoom room, int id)
    {
        base.Initialize(socket, room, id);

        this.msgHandler.Add(PacketType.INPUT, this.HandleInputPacket);
    }

    private void HandleInputPacket(byte[] data)
    {
        var packet = new InputPacket();
        packet.DecodeMessage(data);
        Debug.Log(packet.movement);

        ThreadManager.ExecuteOnMainThread(() => {
            this.PerformMovement(packet);
            this.SendReconcilePacket(packet.tick);
        });

    }

    private void PerformMovement(InputPacket packet){
        var x = packet.movement.x;
        var y = packet.movement.y;

        var lookDir = new Vector3(packet.lookDir.x, 0, packet.lookDir.y);

        var rightDir = new Vector3(packet.rightDir.x, 0, packet.rightDir.y);

        var moveDir = (lookDir * y + rightDir * x);
        var velocity = moveSpd * deltaTime * moveDir.normalized;

        if(x != 0 || y != 0) this.PerformRotateToDir(moveDir);

        this.transform.Translate(velocity, Space.World);
    }

    private void PerformRotateToDir(Vector3 dir){
        dir.Normalize();
        float angle = -Mathf.Atan2(-dir.x, dir.z) * Mathf.Rad2Deg;
        
        var eulers = transform.eulerAngles;
        eulers.y = angle;
        transform.eulerAngles = eulers;
    }

    private void SendReconcilePacket(byte tick){
        var packet = new LocalPlayerStatePacket();
        packet.playerState.position = transform.position;
        packet.playerState.rotation = transform.eulerAngles;
        packet.tick = tick;
        var data = packet.EncodeData();
        this.socket.SendDataUDP(data);
    }
#endif
}
