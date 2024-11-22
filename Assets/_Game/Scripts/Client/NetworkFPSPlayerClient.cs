using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;
using UnityEngine.UIElements;

public partial class NetworkFPSPlayer : Kigor.Networking.NetworkPlayer
{
#if CLIENT_BUILD
    private NetworkCamera cameraController => NetworkCamera.Instance;
    
    private FPSInputPacket pendingInputPacket;
    private FPSPlayerState lastSmoothState;

    private float rawInterpolator;
    
    protected partial void Awake()
    {
        statesBuffer = new FPSPlayerState[TickScheduler.MAX_TICK];
        pendingInputPacket = new FPSInputPacket();
    }
    private void Start()
    {
        this.currentState.position = transform.position;
        this.currentState.horizontalRotation = transform.eulerAngles.y;
        this.currentState.verticalRotation = transform.eulerAngles.x;

        this.stateCounter = 0;
        this.rawInterpolator = 0;

        this.WeaponController.SwitchWeapon(0);
    }
    protected partial void Update()
    {
        if (this.IsLocalPlayer)
        {
            pendingInputPacket.movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (!pendingInputPacket.jump) pendingInputPacket.jump = Input.GetKeyDown(KeyCode.Space);
            pendingInputPacket.shoot = Input.GetMouseButton(0);

            this.Controller.PerformRotation();

            var currentPos = transform.position;
            currentPos.x = Mathf.Lerp(currentPos.x, this.Position.x, Time.deltaTime * this.smoothSpd);
            currentPos.z = Mathf.Lerp(currentPos.z, this.Position.z, Time.deltaTime * this.smoothSpd);
            currentPos.y = Mathf.Lerp(currentPos.y, this.Position.y, Time.deltaTime * (this.smoothSpd + 10));
            this.transform.position = currentPos;
        }
        else
        {
            var maxTimeCounter = this.stateCounter * this.TickScheduler.TickDeltaTime;
            var maxTimeBuffered = (TickScheduler.MAX_TICK) * this.TickScheduler.TickDeltaTime;
            Debug.Log((0, this.rawInterpolator, this.stateCounter, maxTimeCounter));
            // Debug.Log((0, (this.rawInterpolator > maxTimeCounter && this.rawInterpolator - maxTimeCounter < 2),
            //     this.rawInterpolator < maxTimeCounter, this.rawInterpolator == maxTimeCounter));
            if (this.rawInterpolator > maxTimeCounter && this.rawInterpolator - maxTimeCounter < 2 * this.TickScheduler.TickDeltaTime) 
                this.rawInterpolator = maxTimeCounter;
            else if(this.rawInterpolator < maxTimeCounter || (this.rawInterpolator > maxTimeCounter && this.rawInterpolator - maxTimeCounter > 250 * this.TickScheduler.TickDeltaTime))
            {
                this.rawInterpolator += Time.deltaTime;
                if (this.rawInterpolator > maxTimeBuffered) this.rawInterpolator = 0;
            }
            else if (this.rawInterpolator == maxTimeCounter) return;
            
            
            var lastTick = (int)(this.rawInterpolator / this.TickScheduler.TickDeltaTime);
            var nextTick = lastTick == TickScheduler.MAX_TICK - 1 ? 0 : lastTick + 1;
            var lastTickTime = lastTick * this.TickScheduler.TickDeltaTime;
            var interpolator = Mathf.InverseLerp(0, this.TickScheduler.TickDeltaTime, this.rawInterpolator - lastTickTime);
            Debug.Log((1, this.rawInterpolator, this.stateCounter, maxTimeCounter, this.statesBuffer[lastTick].position, this.statesBuffer[nextTick].position, lastTick, nextTick));
            // var interpolator = this.TickScheduler.GetInterpolator(out var lastTick, out var nextTick);
            if (interpolator < 0) return;
            var state = FPSPlayerState.Interpolate(this.statesBuffer[lastTick], this.statesBuffer[nextTick], interpolator);
            
            transform.position = state.position;
            
            var currentRot = transform.eulerAngles;
            currentRot.x = state.verticalRotation;  
            currentRot.y = state.horizontalRotation;
            this.transform.eulerAngles = currentRot;
            
            //var currentHeadRot = this.Avatar.Di
        }
    }
    protected partial void TickUpdate()
    {
        this.Controller.PerformTickMovement(this.pendingInputPacket);

        if (pendingInputPacket.jump)
        {
            Debug.Log("Start jump: " + TickScheduler.CurrentTick + " " + transform.position.y);
            this.Controller.PerformTickJump(pendingInputPacket);
        }

        var state = new FPSPlayerState()
        {
            position = Position,
            horizontalRotation = transform.eulerAngles.y,
        };

        var vertRot = this.Avatar.HeadTransform.transform.eulerAngles.x;
        if (vertRot > 180)
            vertRot -= 360;
        state.verticalRotation = vertRot;
        state.init = true;

        this.statesBuffer[this.TickScheduler.CurrentTick] = state;
        lastTick = this.TickScheduler.CurrentTick;
        this.SendInputPacket();
        pendingInputPacket.jump = false;
    }

    protected override void LocalPlayerSetPostAction()
    {
        this.TickScheduler.RegisterTickCallback(this.TickUpdate);
        NetworkHandleClient.Instance.OnReconcilePacketReceived += this.ServerReconciliation;

        this.cameraController.transform.SetParent(this.Avatar.CamHolder);
        this.cameraController.transform.localPosition = Vector3.zero;
        this.cameraController.transform.localEulerAngles = Vector3.zero;
        
        this.RecursivelyDisableRenderer(this.transform);
        //DL.Utils.CoroutineUtils.Invoke(this, () => this.WeaponController.SwitchWeapon(0), 0);
    }
    private void RecursivelyDisableRenderer(Transform root)
    {
        if (root.name == "Gun Holder Local") return;
        if (root.name == "Hit Impact Container") return;
        if (root.name == "Traces Container") return;

        var renderer = root.GetComponent<Renderer>();
        if (renderer)
            renderer.enabled = false;
        for (int i = 0; i < root.childCount; i++)
        {
            RecursivelyDisableRenderer(root.GetChild(i));
        }
    }

    public void SetNonLocalState(FPSPlayerState state)
    {
        this.stateCounter++;
        if (this.stateCounter >= TickScheduler.MAX_TICK) this.stateCounter = 0;
        this.statesBuffer[this.stateCounter] = state;
        
        // transform.position = state.position;
        // var currentRot = transform.eulerAngles;
        // currentRot.x = state.verticalRotation;  
        // currentRot.y = state.horizontalRotation;
        // this.transform.eulerAngles = currentRot;
    }

    public void ServerReconciliation(int tick, FPSPlayerState state)
    {
        var savedState = this.statesBuffer[tick];
        if (!savedState.init) return;
        
        if (!FPSPlayerState.IsEqual(savedState, state))
        {
            this.room.Rule.TickScheduler.SetTick(tick);
            statesBuffer[tick] = state;
            Debug.Log("Conflict: " + (tick, savedState.position, state.position));
        
            ThreadManager.ExecuteOnMainThread(() =>
            {
                this.Position = state.position;
            });
        }
    }

    private void SendInputPacket()
    {
        pendingInputPacket.tick = (byte)this.TickScheduler.CurrentTick;
        pendingInputPacket.moveDir = new Vector2(transform.forward.x, transform.forward.z);
        pendingInputPacket.cameraAngle = Avatar.HeadTransform.eulerAngles.x;
        NetworkTransport.Instance.SendPacketUDP(pendingInputPacket);
    }

    private void OnDestroy()
    {
        NetworkHandleClient.Instance.OnReconcilePacketReceived -= this.ServerReconciliation;
    }
#endif
}

[System.Serializable]
public struct FPSPlayerState
{
    public Vector3 position;
    public float horizontalRotation, verticalRotation;

    public Vector2 rotation => new Vector2(horizontalRotation, verticalRotation);
    public bool init;

    public static bool IsEqual(FPSPlayerState a, FPSPlayerState b)
    {
        bool posCheck = (a.position - b.position).sqrMagnitude <= 0.021f;
        return posCheck;
    }
    public static float Difference(FPSPlayerState a, FPSPlayerState b)
    {
        return (a.position - b.position).sqrMagnitude;
    }

    public static FPSPlayerState Interpolate(FPSPlayerState a, FPSPlayerState b, float t)
    {
        FPSPlayerState result = new()
        {
            position = Vector3.Lerp(a.position, b.position, t),
            horizontalRotation = Mathf.Lerp(a.horizontalRotation, b.horizontalRotation, t),
            verticalRotation = Mathf.Lerp(a.verticalRotation, b.verticalRotation, t),
        };
        return result;
    }
}
