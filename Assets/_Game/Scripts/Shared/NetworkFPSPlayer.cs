using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Kigor.Networking;

using NetworkPlayer = Kigor.Networking.NetworkPlayer;

public partial class NetworkFPSPlayer : NetworkPlayer
{
    [SerializeField] protected Transform groundCheckPoint;
    [SerializeField] private MeshRenderer bodyMesh;
    [SerializeField] private LayerMask groundMask;


    private FPSPlayerState[] statesBuffer;
    private int stateCounter;
    private FPSPlayerState currentState;

    public TickScheduler TickScheduler => this.room.Rule.TickScheduler;
    public float Height => Mathf.Abs(this.groundCheckPoint.localPosition.y) * this.transform.localScale.y;

    private PlayerAvatar Avatar => this.GetComponent<PlayerAvatar>(); 
    private PlayerController Controller => this.GetComponent<PlayerController>();
    private PhysicsController PhysicsController => this.GetComponent<PhysicsController>();
    private PlayerWeaponController WeaponController => this.GetComponent<PlayerWeaponController>();
    private Vector3 CapsuleTop => transform.position + Vector3.up * this.GetComponent<CapsuleCollider>().height / 2 * transform.localScale.y;
    private Vector3 CapsuleBottom => transform.position - Vector3.up * this.GetComponent<CapsuleCollider>().height / 2 * transform.localScale.y;
    public float CapsuleRadius => Mathf.Max(transform.localScale.x, transform.localScale.z) * this.GetComponent<CapsuleCollider>().radius;

    private int lastTick = -1;
    [SerializeField] private float smoothSpd;

    public Vector3 Position
    {
        get => currentState.position;
        set => currentState.position = value;
    }

    public float HorizontalRotation{
        get => this.currentState.horizontalRotation;
        set => this.currentState.horizontalRotation = value;
    }
    public float VerticalRotation{
        get => this.currentState.verticalRotation;
        set => this.currentState.verticalRotation = value;
    }

    private Vector3 GetGroundCheckPoint(Vector3 parentPos)
    {
        return parentPos + VectorUtils.Multiply(groundCheckPoint.localPosition, groundCheckPoint.localScale);
    }

    protected partial void Awake();
    protected partial void Update();
    protected partial void TickUpdate();

    public (Vector3, Vector3) GetCapsuleEnds(Vector3 center)
    {
        var offset = Vector3.up * (this.GetComponent<CapsuleCollider>().height / 2 * transform.localScale.y - this.CapsuleRadius);
        return (center + offset, center - offset);
    }

    public FPSPlayerState GetState(int stateIndex) => this.statesBuffer[stateIndex];
    public FPSPlayerState GetLastState(out bool found)
    {
        found = lastTick != -1;
        if (found) return this.statesBuffer[lastTick];
        return default;
    }

}
