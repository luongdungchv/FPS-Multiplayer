using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;

public partial class PlayerController
{
#if CLIENT_BUILD
    protected partial void Awake()
    {
    }
    protected partial void TickUpdate()
    {
    }
    protected partial void Update()
    {
        var dir = (this.Player.Position - this.transform.position);
        var dist = dir.sqrMagnitude;
        if(dist < 0.009f) return;
        dir.Normalize();

        transform.position += dir * this.moveSpd * Time.deltaTime;
    }

    public void PerformMovement(FPSInputPacket packet)
    {
        var lookDir = transform.forward;
        var rightDir = new Vector3(lookDir.z, 0, -lookDir.x);

        var x = packet.movement.x;
        var y = packet.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var moveVelocity = moveDir.normalized * moveSpd;

        var jumpVel = packet.jump ? Vector3.up * jumpSpd : Vector3.zero;

        //transform.position += (moveVelocity + jumpVel) * this.Player.TickScheduler.TickDeltaTime;
        this.Player.Position += (moveVelocity + jumpVel) * this.Player.TickScheduler.TickDeltaTime;
        this.PerformVerticalMovement();
    }

    public void PerformRotation()
    {
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");

        var currentRot = transform.eulerAngles;
        currentRot.y += mouseX * this.mouseSen;

        this.PerformHeadRotation(mouseY, mouseSen);

        transform.eulerAngles = currentRot;
    }

    public void PerformHeadRotation(float amount, float sensitivity)
    {
        var currentRot = Avatar.HeadTransform.localEulerAngles;
        var mouseY = amount * sensitivity;

        if (currentRot.x > 180)
            currentRot.x -= 360;
        currentRot.x -= mouseY;
        currentRot.x = Mathf.Clamp(currentRot.x, -90f, 90f);

        Avatar.HeadTransform.localEulerAngles = currentRot;
    }

    public void PerformJump(FPSInputPacket packet)
    {
        if (!inAir)
        {
            this.inAir = true;
            this.currentJump = this.jumpSpd;
        }
    }
    public void PerformVerticalMovement()
    {
        if (inAir)
        {
            this.currentJump -= gravity * this.Player.TickScheduler.TickDeltaTime;
            var vel = Vector3.up * currentJump;
            this.Player.Position += Vector3.up * currentJump;
            var groundCheck = this.Player.GroundCheck(out var groundPos);
            if (currentJump < 0 && groundCheck)
            {
                this.inAir = false;
                this.currentJump = 0;
                this.Player.Position = groundPos + Vector3.up * (this.Player.Height + 0.001f);
                //Debug.Log($"Ground Check: {groundCheck}, {transform.position}");
            }
        }
    }
#endif
}
