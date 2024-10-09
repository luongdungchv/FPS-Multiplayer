using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;

public partial class TestPlayer : Kigor.Networking.NetworkPlayer
{
    [SerializeField] private float moveSpeed;

    [SerializeField] private ClientCameraController cameraController;
#if CLIENT_BUILD
    protected void TickUpdate()
    {
        this.SendPlayerState();
    }

    protected override void LocalPlayerSetPostAction()
    {
        this.room.Rule.TickScheduler.RegisterTickCallback(TickUpdate);
    }

    private void Update(){
        if(this.IsLocalPlayer) this.PerformMovement();
    }

    public void SetCameraController(ClientCameraController cameraController){
        this.cameraController = cameraController;
    }

    private void PerformMovement(){
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");

        var lookDir = cameraController.transform.forward;
        lookDir.y = 0;
        lookDir.Normalize();

        var rightDir = cameraController.transform.right;
        rightDir.y = 0;
        rightDir.Normalize();

        var moveDir = (lookDir * y + rightDir * x);
        var velocity = moveSpeed * Time.deltaTime * moveDir.normalized;

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

    private void SendPlayerState(){
        var packet = new PlayerStatePacket();
        packet.position = this.transform.position;
        packet.rotation = this.transform.eulerAngles;
        
        try{
            NetworkTransport.Instance.SendPacketUDP(packet);
        }
        catch(System.Exception e){
            Debug.Log(e);
        }
    }

#endif
}
