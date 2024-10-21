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
        var lookDir = transform.forward;
        var rightDir = new Vector3(lookDir.z, 0, -lookDir.x);

        var x = packet.movement.x;
        var y = packet.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var vel = moveDir.normalized * moveSpd * this.TickScheduler.TickDeltaTime;

        var ground = Vector3.zero;
        if (!this.inAir) vel = this.PhysicsController.GetMoveVector(transform.position, vel, out ground);
        vel += this.PerformVerticalMovement(packet);

        var lastPos = transform.position;
        transform.position += vel;

        var hitNormals = this.PhysicsController.DetectCollision(lastPos, transform.position, out int hitCount, out var touchGround, out var groundPos, out var firstTouchPos);
        if (hitCount > 0)
        {
            if (this.currentJump < 0 && touchGround && this.inAir)
            {
                this.inAir = false;
                this.currentJump = 0;
                lastPos = groundPos;
            }

            for (int i = 0; i < hitCount; i++)
            {
                var normal = hitNormals[i];
                if (normal == Vector3.zero) continue;
                var cross = Vector3.Cross(normal, vel);
                vel = Vector3.Cross(cross, normal);
                lastPos += vel;
            }
            transform.position = lastPos;
        }
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
    public Vector3 PerformVerticalMovement(FPSInputPacket packet)
    {
         if (inAir)
        {
            this.currentJump -= gravity * this.TickScheduler.TickDeltaTime;
            return Vector3.up * currentJump * this.TickScheduler.TickDeltaTime;
        }
        return Vector3.zero;
    }
#endif
}
