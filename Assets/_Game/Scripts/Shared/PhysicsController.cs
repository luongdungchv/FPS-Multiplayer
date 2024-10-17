using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsController : MonoBehaviour
{
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private int raycastBufferSize;
    [SerializeField] private LayerMask mask, groundMask;
    [SerializeField] private float slopeAngle;
    private NetworkFPSPlayer Player => this.GetComponent<NetworkFPSPlayer>();

    private RaycastHit[] raycastBuffer;
    private Vector3[] hitNormalsBuffer;
    private int raycastCount;
    private float distToGround;
    private float minSlopeDot;

    private void Awake()
    {
        raycastBuffer = new RaycastHit[raycastBufferSize];
        this.hitNormalsBuffer = new Vector3[raycastBufferSize];

        var radius = this.Player.CapsuleRadius;
        this.distToGround = radius / Mathf.Cos(this.slopeAngle * Mathf.Deg2Rad) - radius;
        this.minSlopeDot = Mathf.Cos((90 - slopeAngle) * Mathf.Deg2Rad);
    }

    public bool DetectCollision(out Vector3 hitNormal, out bool touchGround, out Vector3 touchPos)
    {
        var lastState = this.Player.GetLastState(out bool found);

        hitNormal = Vector3.zero;
        touchGround = false;
        touchPos = Vector3.zero;

        if (lastState.init)
        {
            var (top, bot) = this.Player.GetCapsuleEnds(lastState.position);
            var dir = this.Player.Position - lastState.position;
            var physicsScene = this.Player.CurrentPhysicsScene;
            var detected = physicsScene.CapsuleCast(top, bot, this.Player.CapsuleRadius, dir, raycastBuffer, dir.magnitude + 0.1f, this.mask);

            // Debug.Log((this.Player.Position, lastState.position));        

            if (detected == 0) return false;
            if (detected > 0)
            {
                for (int i = 0; i < detected; i++)
                {
                    var hitInfo = raycastBuffer[i];
                    hitNormal += hitInfo.normal;
                    var angle = 90 - Vector3.Angle(Vector3.up, hitInfo.normal);
                    if (angle > this.slopeAngle)
                    {
                        touchGround = true;
                        touchPos = lastState.position + dir * hitInfo.distance;
                    }
                }
            }
        }
        return true;
    }

    public bool DetectCollision(Vector3 startPos, Vector3 endPos, out Vector3 hitNormal, out bool touchGround, out Vector3 groundPos)
    {
        hitNormal = Vector3.zero;
        touchGround = false;
        groundPos = Vector3.zero;


        var (top, bot) = this.Player.GetCapsuleEnds(startPos);
        var dir = endPos - startPos;
        var physicsScene = this.Player.CurrentPhysicsScene;
        var detected = physicsScene.CapsuleCast(top, bot, this.Player.CapsuleRadius, dir, raycastBuffer, dir.magnitude + 0.1f, this.mask);

        // Debug.Log((this.Player.Position, lastState.position));        

        if (detected == 0) return false;
        if (detected > 0)
        {
            for (int i = 0; i < detected; i++)
            {
                var hitInfo = raycastBuffer[i];
                hitNormal += hitInfo.normal;
                var angle = 90 - Vector3.Angle(Vector3.up, hitInfo.normal);
                if (angle > this.slopeAngle)
                {
                    touchGround = true;
                    groundPos = startPos + dir * hitInfo.distance;
                }
            }
        }

        return true;
    }

    public Vector3[] DetectCollision(out int collisionCount, out bool touchGround, out Vector3 groundPos){
        var lastState = this.Player.GetLastState(out bool found);

        touchGround = false;
        collisionCount = 0;
        groundPos = Vector3.zero;

        if (lastState.init)
        {
            var (top, bot) = this.Player.GetCapsuleEnds(lastState.position);
            var dir = this.Player.Position - lastState.position;
            var physicsScene = this.Player.CurrentPhysicsScene;
            collisionCount = physicsScene.CapsuleCast(top, bot, this.Player.CapsuleRadius, dir.normalized, raycastBuffer, dir.magnitude + 0.1f, this.mask);

            // Debug.Log((this.Player.Position, lastState.position));        

            if (collisionCount <= 0) return null;
            if (collisionCount > 0)
            {
                for (int i = 0; i < collisionCount; i++)
                {
                    var hitInfo = raycastBuffer[i];
                    if(hitInfo.normal.normalized == -dir.normalized) continue;
                    var normal = (Vector3.Dot(hitInfo.normal.normalized, Vector3.up) < minSlopeDot) ? 
                        hitInfo.normal.normalized : 
                        hitInfo.normal.Set(y: 0).normalized;
                    hitNormalsBuffer[i] = normal;
                    var angle = 90 - Vector3.Angle(Vector3.up, hitInfo.normal);
                    if (angle > this.slopeAngle)
                    {
                        touchGround = true;
                        groundPos = lastState.position + dir * hitInfo.distance;
                    }
                }
            }
        }
        return this.hitNormalsBuffer;
    }

    public Vector3 GetMoveVector(Vector3 velocity){
        var isGround = Physics.Raycast(groundCheckPoint.position, Vector3.down, out var hitInfo, this.distToGround, this.groundMask);
        if(isGround){
            var tangent = Vector3.Cross(hitInfo.normal, velocity);
            var vel = Vector3.Cross(tangent, hitInfo.normal);
            Debug.Log(vel.normalized * velocity.magnitude);
            return vel.normalized * velocity.magnitude;
        }
        return velocity;
    }
}


