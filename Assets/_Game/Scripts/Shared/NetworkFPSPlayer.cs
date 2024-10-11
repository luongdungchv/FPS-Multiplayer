using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Kigor.Networking;

using NetworkPlayer = Kigor.Networking.NetworkPlayer;

public partial class NetworkFPSPlayer : NetworkPlayer
{
    [SerializeField] private float moveSpd, mouseSen, jumpSpd, gravity;
    [SerializeField] protected Transform headTransform, camHolder, groundCheckPoint;
    [SerializeField] private MeshRenderer bodyMesh;
    [SerializeField] private LayerMask groundMask;


    private TickScheduler tickScheduler => this.room.Rule.TickScheduler;
    private float height => Mathf.Abs(this.groundCheckPoint.localPosition.y) * this.transform.localScale.y;

    protected partial void Awake();
    protected partial void Update();
    protected partial void TickUpdate();

    
}
