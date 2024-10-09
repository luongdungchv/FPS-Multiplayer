using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Kigor.Networking;

using NetworkPlayer = Kigor.Networking.NetworkPlayer;

public partial class NetworkFPSPlayer : NetworkPlayer
{
    [SerializeField] private float moveSpd, mouseSen;
    [SerializeField] protected Transform headTransform, camHolder;
    [SerializeField] private MeshRenderer bodyMesh;


    private TickScheduler tickScheduler => this.room.Rule.TickScheduler;

    protected partial void Awake();
    protected partial void Update();
    protected partial void TickUpdate();

    
}
