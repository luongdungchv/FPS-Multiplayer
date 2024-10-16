using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsController : MonoBehaviour
{
    [SerializeField] private int raycastBufferSize;
    [SerializeField] private LayerMask mask;
    [SerializeField] private float slopeAngle;
    private NetworkFPSPlayer Player => this.GetComponent<NetworkFPSPlayer>();

    private RaycastHit[] raycastBuffer;
    private int raycastCount;

    private void Awake(){
        raycastBuffer = new RaycastHit[raycastBufferSize];
    }

    public bool DetectCollision(out Vector3 hitNormal, out bool touchGround, out Vector3 groundPos){
        var lastState = this.Player.GetLastState(out bool found);

        hitNormal = Vector3.zero;
        touchGround = false;
        groundPos = Vector3.zero;

        if(lastState.init){
            var (top, bot) = this.Player.GetCapsuleEnds(lastState.position);
            var dir = (this.Player.Position - lastState.position);
            var physicsScene = this.Player.CurrentPhysicsScene;
            var detected = physicsScene.CapsuleCast(top, bot, this.Player.CapsuleRadius, dir, raycastBuffer, dir.magnitude + 0.1f, this.mask);
            if(detected == 0) return false;
            if(detected > 0){
                for(int i = 0; i < detected; i++){
                    var hitInfo = raycastBuffer[i];
                    hitNormal += hitInfo.normal;
                    var angle = 90 - Vector3.Angle(Vector3.up, hitInfo.normal);
                    if(angle > this.slopeAngle){
                        touchGround = true;
                        groundPos = hitInfo.point;
                    }
                }   
            }
        }
        return true;
    }

}
