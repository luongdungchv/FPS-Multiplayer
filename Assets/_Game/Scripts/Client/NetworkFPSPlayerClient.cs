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
    }
    protected partial void Update()
    {
        if (!this.IsLocalPlayer) return;
        pendingInputPacket.movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (!pendingInputPacket.jump) pendingInputPacket.jump = Input.GetKeyDown(KeyCode.Space);
        if (!pendingInputPacket.shoot) pendingInputPacket.shoot = Input.GetMouseButton(0);

        this.Controller.PerformRotation();

        var currentPos = transform.position;
        currentPos.x = Mathf.Lerp(currentPos.x, this.Position.x, Time.deltaTime * this.smoothSpd);
        currentPos.z = Mathf.Lerp(currentPos.z, this.Position.z, Time.deltaTime * this.smoothSpd);
        currentPos.y = Mathf.Lerp(currentPos.y, this.Position.y, Time.deltaTime * (this.smoothSpd + 10));
        this.transform.position = currentPos;
    }
    protected partial void TickUpdate()
    {
        this.Controller.PerformTickMovement(pendingInputPacket);

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

        this.cameraController.transform.SetParent(this.Avatar.HeadTransform);
        this.cameraController.transform.localPosition = Vector3.zero;
        this.cameraController.transform.localEulerAngles = Vector3.zero;

        this.RecursivelyDisableRenderer(this.transform);
    }
    private void RecursivelyDisableRenderer(Transform root)
    {
        if (root.name == "Gun Holder Local") return;

        var renderer = root.GetComponent<Renderer>();
        if (renderer)
            renderer.enabled = false;
        for (int i = 0; i < root.childCount; i++)
        {
            RecursivelyDisableRenderer(root.GetChild(i));
        }
    }
    public bool GroundCheck(out Vector3 groundPos)
    {
        var castPoint = this.GetGroundCheckPoint(Position);
        var currentCheck = Physics.Raycast(castPoint, Vector3.down, out var hitInfo, 0.1f, this.groundMask);
        groundPos = hitInfo.point;
        //var lastTick = this.TickScheduler.GetLastTicks(1)[0];
        var lastState = this.statesBuffer[lastTick];
        if (lastState.init)
        {
            var lastGroundPos = this.GetGroundCheckPoint(lastState.position);
            var check = Physics.Raycast(lastGroundPos, castPoint - lastGroundPos, out hitInfo, (castPoint - lastGroundPos).magnitude + 0.1f, this.groundMask);
            Debug.Log((Position.y, castPoint.y, lastGroundPos.y));
            if (check)
            {
                currentCheck = true;
                groundPos = hitInfo.point;
            }
        }
        return currentCheck;
    }

    public bool SmoothGroundCheck(out Vector3 groundPos)
    {
        var startPos = lastSmoothState.position;
        var endPos = transform.position;
        var collide = this.PhysicsController.DetectCollision(startPos, endPos, out Vector3 hitNormal, out var groundCheck, out groundPos);
        return groundCheck;
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

    private void SendShootPacket()
    {
        var packet = new FPSShootPacket();
        packet.shootDir = NetworkCamera.Instance.transform.forward;
        NetworkTransport.Instance.SendPacketTCP(packet);
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
}
