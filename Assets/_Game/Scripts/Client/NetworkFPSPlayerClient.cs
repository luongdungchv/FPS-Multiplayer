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
        pendingInputPacket = new FPSInputPacket();
    }
    protected partial void Update()
    {
        if (!this.IsLocalPlayer) return;
        pendingInputPacket.movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        pendingInputPacket.jump = Input.GetKeyDown(KeyCode.Space);
        pendingInputPacket.shoot = Input.GetMouseButtonDown(0);
    }
    protected partial void TickUpdate()
    {
        this.SendInput();
        this.PerformRotation();
        this.PerformMovement();

        var state = new FPSPlayerState(){
            position = transform.position,
            horizontalRotation = transform.eulerAngles.y,
        };

        var vertRot = this.headTransform.transform.eulerAngles.x;
        if(vertRot > 180)
            vertRot -= 360;
        state.verticalRotation = vertRot;

        this.statesBuffer[this.tickScheduler.CurrentTick] = state;

    }

    protected override void LocalPlayerSetPostAction()
    {
        this.tickScheduler.RegisterTickCallback(this.TickUpdate);
        NetworkHandleClient.Instance.OnReconcilePacketReceived += this.ServerReconciliation;

        this.cameraController.transform.SetParent(this.camHolder);
        this.cameraController.transform.localPosition = Vector3.zero;
        this.cameraController.transform.localEulerAngles = Vector3.zero;

        this.RecursivelyDisableRenderer(this.transform);
    }
    private void RecursivelyDisableRenderer(Transform root){
        var renderer = root.GetComponent<Renderer>();
        if(renderer != null)
            renderer.enabled = false;
        for (int i = 0; i < root.childCount; i++)
        {
            RecursivelyDisableRenderer(root.GetChild(i));   
        }    
    }
    private void SendInput()
    {
        pendingInputPacket.tick = (byte)this.tickScheduler.CurrentTick;
        pendingInputPacket.moveDir = new Vector2(transform.forward.x, transform.forward.z);
        pendingInputPacket.cameraAngle = headTransform.eulerAngles.x;
        NetworkTransport.Instance.SendPacketUDP(pendingInputPacket);
    }

    private void PerformMovement()
    {
        var lookDir = transform.forward;
        var rightDir = new Vector3(lookDir.z, 0, -lookDir.x);

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

    public void ServerReconciliation(int tick, FPSPlayerState state)
    {
        // var savedState = this.statesBuffer[tick];

        // if (!FPSPlayerState.IsEqual(savedState, state))
        // {
        //     this.room.Rule.TickScheduler.SetTick(tick);
        //     statesBuffer[tick] = state;

        //     ThreadManager.ExecuteOnMainThread(() =>
        //     {
        //         this.transform.position = state.position;

        //         // var currentRot = this.transform.eulerAngles;
        //         // currentRot.y = state.horizontalRotation;

        //         // this.transform.eulerAngles = currentRot;
        //     });
        // }
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
        bool posCheck = (a.position - b.position).sqrMagnitude <= 0.05f;
        return posCheck;
    }
}
