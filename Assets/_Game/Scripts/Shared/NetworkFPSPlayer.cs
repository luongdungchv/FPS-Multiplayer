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

    public TickScheduler TickScheduler => this.room.Rule.TickScheduler;
    public float Height => Mathf.Abs(this.groundCheckPoint.localPosition.y) * this.transform.localScale.y;
    private PlayerAvatar Avatar => this.GetComponent<PlayerAvatar>();
    private PlayerController Controller => this.GetComponent<PlayerController>();

    private Vector3 GetGroundCheckPoint(Vector3 parentPos){
        return parentPos + VectorUtils.Multiply(groundCheckPoint.localPosition, groundCheckPoint.localScale);
    }

    protected partial void Awake();
    protected partial void Update();
    protected partial void TickUpdate();

    
}
