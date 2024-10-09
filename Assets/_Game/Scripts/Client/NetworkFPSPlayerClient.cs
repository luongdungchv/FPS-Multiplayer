using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;

public partial class NetworkFPSPlayer : Kigor.Networking.NetworkPlayer
{
#if CLIENT_BUILD
    private NetworkCamera cameraController => NetworkCamera.Instance;
    private FPSInputPacket pendingInputPacket;
    private FPSPlayerState[] statesBuffer;
    protected partial void Awake()
    {
        statesBuffer = new FPSPlayerState[TickScheduler.MAX_TICK];
    }
    protected partial void Update()
    {
        if (!this.IsLocalPlayer) return;
        pendingInputPacket.movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        pendingInputPacket.jump = Input.GetKeyDown(KeyCode.Space);
        pendingInputPacket.shoot = Input.GetMouseButtonDown(0);

        pendingInputPacket.cameraAngle = cameraController.transform.eulerAngles.x;
    }
    protected partial void TickUpdate()
    {
        this.SendInput();
        this.PerformMovement();
        this.PerformRotation();

        var state = new FPSPlayerState(){
            position = transform.position,
            horizontalRotation = transform.eulerAngles.y,
        };

        var vertRot = this.cameraController.transform.eulerAngles.x;
        if(vertRot > 180)
            vertRot -= 360;
        state.verticalRotation = vertRot;

        this.statesBuffer[this.tickScheduler.CurrentTick] = state;

    }

    protected override void LocalPlayerSetPostAction()
    {
        this.tickScheduler.RegisterTickCallback(this.TickUpdate);
        NetworkHandleClient.Instance.OnReconcilePacketReceived += this.ServerReconciliation;
    }
    private void SendInput()
    {
        pendingInputPacket.tick = (byte)this.tickScheduler.CurrentTick;
        NetworkTransport.Instance.SendPacketUDP(pendingInputPacket);
    }

    private void PerformMovement()
    {
        var lookDir = transform.forward;
        var rightDir = transform.right;

        var x = pendingInputPacket.movement.x;
        var y = pendingInputPacket.movement.y;

        var moveDir = lookDir * y + rightDir * x;
        var velocity = moveDir.normalized * moveSpd;

        transform.position += velocity * this.tickScheduler.TickDeltaTime;
    }

    private void PerformRotation()
    {
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");
        
        var currentRot = transform.eulerAngles;
        currentRot.y += mouseX * this.mouseSen;

        (this.cameraController as FPSCameraController).PerformVerticalRotation(mouseY, this.mouseSen);

    }

    public void ServerReconciliation(int tick, FPSPlayerState state)
    {
        var savedState = this.statesBuffer[tick];
        Debug.Log((tick, state.position, state.rotation));

        if (!FPSPlayerState.IsEqual(savedState, state))
        {
            this.room.Rule.TickScheduler.SetTick(tick);
            statesBuffer[tick] = state;

            ThreadManager.ExecuteOnMainThread(() =>
            {
                this.transform.position = state.position;
                this.transform.eulerAngles = state.rotation;
            });
        }
    }

    private void OnDestroy(){
        NetworkHandleClient.Instance.OnReconcilePacketReceived -= this.ServerReconciliation;
    }
#endif
}

public struct FPSPlayerState
{
    public Vector3 position;
    public float horizontalRotation, verticalRotation;

    public Vector2 rotation => new Vector2(horizontalRotation, verticalRotation);

    public static bool IsEqual(FPSPlayerState a, FPSPlayerState b)
    {
        bool posCheck = (a.position - b.position).sqrMagnitude <= 0.01f;
        bool rotCheck = (a.rotation - b.rotation).sqrMagnitude <= 0.01f;
        return posCheck && rotCheck;
    }
}
