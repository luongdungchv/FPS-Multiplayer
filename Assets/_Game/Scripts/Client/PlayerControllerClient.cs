using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;
using System.Linq;
using Unity.VisualScripting;
public partial class PlayerController
{
#if CLIENT_BUILD
    private float smoothCurrentJump;
    private bool smoothInAir;
    protected partial void Awake()
    {
    }
    protected partial void TickUpdate()
    {
    }
    protected partial void Update()
    {
        // var dir = this.Player.Position - this.transform.position;
        // var dist = dir.sqrMagnitude;
        // if(dist < 0.009f) return;
        // dir.Normalize();

        // transform.position += dir * this.moveSpd * Time.deltaTime;
        //transform.position = this.Player.Position;

    }

    public void PerformTickMovement(FPSInputPacket packet)
    {
        var lookDir = transform.forward;
        var rightDir = new Vector3(lookDir.z, 0, -lookDir.x);

        var x = packet.movement.x;
        var y = packet.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var moveVelocity = moveDir.normalized * moveSpd;

        var vel = moveVelocity * this.Player.TickScheduler.TickDeltaTime;
        if (!this.inAir) vel = this.PhysicsController.GetMoveVector(this.Player.Position, vel, out var currentGround);
        vel += this.PerformTickVerticalMovement(packet.tick);

        var lastPos = this.Player.Position;
        this.Player.Position += vel;

        var hitNormals = this.PhysicsController.DetectCollision(lastPos, Player.Position, out int hitCount, out var touchGround, out var groundPos, out var touchPos, packet.tick);
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
                var normal = hitNormals[i].XYZ();
                var factor = hitNormals[i].w;
                if (normal == Vector3.zero) continue;
                var cross = Vector3.Cross(normal, vel);
                var oldVel = vel;
                vel = Vector3.Cross(cross, normal);
            }
            lastPos += vel;
            
            this.Player.Position = lastPos;
        }

        // this.transform.position = this.Player.Position;

        if (currentJump == 0 && !this.PhysicsController.IsGrounded(Player.Position) && !inAir)
        {
            this.PerformTickFall();
        }
    }

    public void PerformMovement(FPSInputPacket packet)
    {
        var lookDir = transform.forward;
        var rightDir = new Vector3(lookDir.z, 0, -lookDir.x);

        var x = packet.movement.x;
        var y = packet.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var vel = moveDir.normalized * moveSpd * Time.deltaTime;

        var ground = Vector3.zero;
        if (!this.smoothInAir) vel = this.PhysicsController.GetMoveVector(transform.position, vel, out ground);
        vel += this.PerformVerticalMovement(packet);

        var lastPos = transform.position;
        transform.position += vel;

        var hitNormals = this.PhysicsController.DetectCollision(lastPos, transform.position, out int hitCount, out var touchGround, out var groundPos, out var firstTouchPos);
        if (hitCount > 0)
        {
            if (this.smoothCurrentJump < 0 && touchGround && this.smoothInAir)
            {
                this.smoothInAir = false;
                this.smoothCurrentJump = 0;
                lastPos = groundPos;
            }

            for (int i = 0; i < hitCount; i++)
            {
                var normal = hitNormals[i].XYZ();
                var factor = hitNormals[i].w;
                if (normal == Vector3.zero || factor == 1) continue;
                var cross = Vector3.Cross(normal, vel);
                vel = Vector3.Cross(cross, normal);
                lastPos += vel;
            }
            transform.position = lastPos;
        }
    }

    public void PerformTickRotation()
    {
        var mouseX = Input.GetAxis("Mouse X") * this.Player.TickScheduler.TickDeltaTime;
        var mouseY = Input.GetAxis("Mouse Y") * this.Player.TickScheduler.TickDeltaTime;

        this.Player.HorizontalRotation += mouseX * mouseSen;
        this.PerformTickHeadRotation(mouseY, mouseSen);

        transform.eulerAngles = transform.eulerAngles.Set(y: this.Player.HorizontalRotation);
        this.Avatar.HeadTransform.eulerAngles = Avatar.HeadTransform.eulerAngles.Set(x: this.Player.VerticalRotation);
    }


    public void PerformTickHeadRotation(float amount, float sensitivity)
    {
        var mouseY = amount * sensitivity;
        if (this.Player.VerticalRotation > 180)
            this.Player.VerticalRotation -= 360;
        this.Player.VerticalRotation -= mouseY;
        this.Player.VerticalRotation = Mathf.Clamp(this.Player.VerticalRotation, -90f, 90f);
    }

    public void PerformRotation()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");

        var currentRot = transform.eulerAngles;
        currentRot.y += mouseX * this.mouseSen;

        this.PerformHeadRotation(mouseY, mouseSen);

        transform.eulerAngles = currentRot;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    public void PerformTickJump(FPSInputPacket packet)
    {
        if (!inAir)
        {
            this.inAir = true;
            this.currentJump = this.jumpSpd;
        }
    }
    public void PerformTickFall()
    {
        this.currentJump = 0;
        this.inAir = true;
    }
    public void PerformJump(FPSInputPacket packet)
    {
        if (!smoothInAir)
        {
            this.smoothInAir = true;
            this.smoothCurrentJump = this.jumpSpd;
        }
    }
    public Vector3 PerformTickVerticalMovement(int tick)
    {
        if (inAir)
        {
            this.currentJump -= gravity * this.Player.TickScheduler.TickDeltaTime;
            var vel = Vector3.up * currentJump;
            // transform.position += Vector3.up * currentJump;
            //this.Player.Position += Vector3.up * currentJump * this.Player.TickScheduler.TickDeltaTime;
            //var groundCheck = this.Player.GroundCheck(out var groundPos);
            return Vector3.up * (this.currentJump * this.Player.TickScheduler.TickDeltaTime);
        }
        return Vector3.zero;
    }

    public Vector3 PerformVerticalMovement(FPSInputPacket packet)
    {
        if (smoothInAir)
        {
            this.smoothCurrentJump -= gravity * Time.deltaTime;
            //transform.position += Vector3.up * smoothCurrentJump * Time.deltaTime;
            //var groundCheck = this.Player.SmoothGroundCheck(out var groundPos);
            return Vector3.up * smoothCurrentJump * Time.deltaTime;
        }
        return Vector3.zero;
    }

    private void OnDestroy()
    {
        if (this.Player.IsLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
#endif
}
