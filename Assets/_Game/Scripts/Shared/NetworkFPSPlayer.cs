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


    private FPSPlayerState[] statesBuffer;
    private float currentJump;
    private bool inAir;

    private TickScheduler tickScheduler => this.room.Rule.TickScheduler;
    private float height => Mathf.Abs(this.groundCheckPoint.localPosition.y) * this.transform.localScale.y;

    protected partial void Awake();
    protected partial void Update();
    protected partial void TickUpdate();

    private bool GroundCheck(out Vector3 groundPos)
    {
        var currentCheck = Physics.Raycast(groundCheckPoint.position, Vector3.down, out var hitInfo, 0.1f, this.groundMask);
        groundPos = hitInfo.point;
        var lastState = this.statesBuffer[this.tickScheduler.GetLastTicks(1)[0]];
        if(lastState.init){
            var lastGroundPos = lastState.position + VectorUtils.Multiply(groundCheckPoint.localPosition, transform.localScale);
            var check = Physics.Linecast(lastGroundPos, groundCheckPoint.position, out hitInfo, this.groundMask); 
            //Debug.Log((groundCheckPoint.position.y, lastGroundPos.y, check));
            if(check){
                currentCheck = currentCheck || check;
                groundPos = hitInfo.point;
            }
        } 
        return currentCheck;
    }

    
}
