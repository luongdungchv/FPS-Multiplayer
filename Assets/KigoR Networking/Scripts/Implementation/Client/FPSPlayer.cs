using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;

using NetworkPlayer = Kigor.Networking.NetworkPlayer;

public partial class FPSPlayer : NetworkPlayer
{
    [SerializeField] private float moveSpd;
    private PlayerState[] states;
#if CLIENT_BUILD
    private NetworkCamera cameraController => NetworkCamera.Instance;
    private InputPacket pendingInputPacket;

    private void Awake()
    {
        pendingInputPacket = new InputPacket();
        this.states = new PlayerState[TickScheduler.MAX_TICK];
        
    }
    private void Update()
    {
        if(!this.IsLocalPlayer) return;
        pendingInputPacket.movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        pendingInputPacket.jump = Input.GetKeyDown(KeyCode.Space);
        pendingInputPacket.shoot = Input.GetMouseButtonDown(0);

        var lookDir = cameraController.transform.forward;
        pendingInputPacket.lookDir = new Vector2(lookDir.x, lookDir.z).normalized;

        var rightDir = cameraController.transform.right;
        pendingInputPacket.rightDir = new Vector2(rightDir.x, rightDir.z).normalized;
    }

    protected void TickUpdate()
    {
        this.SendInput();
        this.ProcessInputPrediction();
        var currentTick = this.room.Rule.TickScheduler.CurrentTick;

        var state = new PlayerState()
        {
            position = transform.position,
            rotation = transform.eulerAngles
        };
        this.states[currentTick] = state;
    }

    protected override void LocalPlayerSetPostAction()
    {
        this.room.Rule.TickScheduler.RegisterTickCallback(TickUpdate);
        NetworkHandleClient.Instance.OnReconcileStateReceived += this.ServerReconciliation;
    }

    private void SendInput()
    {
        pendingInputPacket.tick = (byte)this.room.Rule.TickScheduler.CurrentTick;
        NetworkTransport.Instance.SendPacketUDP(pendingInputPacket);
    }

    private void ProcessInputPrediction()
    {
        this.PerformMovement();
    }
    private void PerformMovement()
    {
        var x = pendingInputPacket.movement.x;
        var y = pendingInputPacket.movement.y;

        var lookDir = cameraController.transform.forward;
        lookDir.y = 0;
        lookDir.Normalize();

        var rightDir = cameraController.transform.right;
        rightDir.y = 0;
        rightDir.Normalize();

        var moveDir = (lookDir * y + rightDir * x);
        var velocity = moveSpd * this.room.Rule.TickScheduler.TickDeltaTime * moveDir.normalized;

        if (x != 0 || y != 0) this.PerformRotateToDir(moveDir);

        this.transform.position += velocity;
    }
    private void PerformRotateToDir(Vector3 dir)
    {
        dir.Normalize();
        float angle = -Mathf.Atan2(-dir.x, dir.z) * Mathf.Rad2Deg;

        var eulers = transform.eulerAngles;
        eulers.y = angle;
        transform.eulerAngles = eulers;
    }

    public void ServerReconciliation(int tick, PlayerState state)
    {
        var savedState = this.states[tick];
        Debug.Log((tick, state.position, state.rotation));
        if (savedState == null)
        {
            return;
        }

        if (savedState != state)
        {
            this.room.Rule.TickScheduler.SetTick(tick);
            states[tick] = state;

            ThreadManager.ExecuteOnMainThread(() =>
            {
                this.transform.position = state.position;
                this.transform.eulerAngles = state.rotation;
            });
        }
    }

    private void OnDestroy()
    {
        NetworkHandleClient.Instance.OnReconcileStateReceived -= this.ServerReconciliation;
    }
#endif

}
public struct PlayerState
{
    public Vector3 position;
    public Vector3 rotation;

    public static bool operator ==(PlayerState state1, PlayerState state2)
    {
        return (state1.position == state2.position && state1.rotation == state2.rotation);
    }
    public static bool operator !=(PlayerState state1, PlayerState state2)
    {
        return (state1.position != state2.position || state1.rotation != state2.rotation);
    }

    public override bool Equals(object obj)
    {
        return this == (PlayerState)obj;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
