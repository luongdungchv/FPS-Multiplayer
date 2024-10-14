using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;

public partial class PlayerController
{
#if SERVER_BUILD
    protected partial void Awake()
    {

    }
    protected partial void TickUpdate()
    {

    }
    protected partial void Update()
    {

    }

    public void PerformMovement(FPSInputPacket packet)
    {
        var lookDir = new Vector3(packet.moveDir.x, 0, packet.moveDir.y);
        var rightDir = new Vector3(lookDir.z, 0, -lookDir.x);

        var x = packet.movement.x;
        var y = packet.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var velocity = moveDir.normalized * moveSpd;

        transform.position += velocity * this.TickScheduler.TickDeltaTime;
    }

    public void PerformRotation(FPSInputPacket packet)
    {
        var characterAngle = Mathf.Atan2(packet.moveDir.x, packet.moveDir.y) * Mathf.Rad2Deg;
        var cameraAngle = packet.cameraAngle;

        var currentRot = transform.eulerAngles;
        currentRot.y = characterAngle;
        transform.eulerAngles = currentRot;

        var currentHeadRot = Avatar.HeadTransform.localEulerAngles;
        currentHeadRot.x = cameraAngle;
        Avatar.HeadTransform.localEulerAngles = currentHeadRot;

    }
    public void PerformJump(FPSInputPacket packet)
    {
        if (!inAir)
        {
            this.inAir = true;
            this.currentJump = this.jumpSpd;
        }
        packet.jump = false;
    }
    public void PerformVerticalMovement(FPSInputPacket packet)
    {
        if (inAir)
        {
            this.currentJump -= gravity * this.TickScheduler.TickDeltaTime;
            var vel = Vector3.up * currentJump;
            transform.position += Vector3.up * currentJump * this.TickScheduler.TickDeltaTime;
            var groundCheck = this.Player.GroundCheck(packet.tick, out var groundPos);
            if (currentJump < 0 && groundCheck)
            {
                this.inAir = false;
                this.currentJump = 0;
                transform.position = groundPos + Vector3.up * (this.Player.Height + 0.001f);
                Debug.Log($"Ground Check: {groundCheck}, {transform.position}");
            }
        }
    }
#endif
}
