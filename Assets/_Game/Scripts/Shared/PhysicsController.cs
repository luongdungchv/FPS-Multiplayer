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
    [SerializeField] private float distToGround;
    [SerializeField] private float minSlopeDot;

    private void Awake()
    {
        raycastBuffer = new RaycastHit[raycastBufferSize];
        this.hitNormalsBuffer = new Vector3[raycastBufferSize];

        var radius = this.Player.CapsuleRadius;
        this.distToGround = radius / Mathf.Cos(this.slopeAngle * Mathf.Deg2Rad) - radius + 0.3f;
        this.minSlopeDot = Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
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

    public Vector3[] DetectCollision(out int collisionCount, out bool touchGround, out Vector3 groundPos, out Vector3 firstTouchPos)
    {
        var lastState = this.Player.GetLastState(out bool found);

        touchGround = false;
        collisionCount = 0;
        groundPos = Vector3.zero;
        firstTouchPos = Vector3.zero;

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
                    if (hitInfo.normal.normalized == -dir.normalized && hitInfo.distance == 0) continue;
                    var canMoveOnSlope = Vector3.Dot(hitInfo.normal.normalized, Vector3.up) > minSlopeDot;
                    var normal = canMoveOnSlope ?
                        hitInfo.normal.normalized :
                        hitInfo.normal.Set(y: 0).normalized;
                    Debug.Log((canMoveOnSlope, normal, hitInfo.normal, hitInfo.collider));
                    hitNormalsBuffer[i] = normal;
                    var angle = 90 - Vector3.Angle(Vector3.up, hitInfo.normal);
                    var touchPos = lastState.position + dir * hitInfo.distance;
                    if (angle > this.slopeAngle)
                    {
                        touchGround = true;
                        groundPos = touchPos;
                    }
                    if(i == 0) firstTouchPos = touchPos;
                }
            }
        }
        return this.hitNormalsBuffer;
    }

    public Vector3[] DetectCollision(Vector3 start, Vector3 end, out int collisionCount, out bool touchGround, out Vector3 groundPos, out Vector3 firstTouchPos, int tick = -2)
    {
        touchGround = false;
        groundPos = Vector3.zero;
        firstTouchPos = start;

        var (top, bot) = this.Player.GetCapsuleEnds(start);
        var dir =  end - start;
        var physicsScene = this.Player.CurrentPhysicsScene;
        var tmpCount = physicsScene.CapsuleCast(top, bot, this.Player.CapsuleRadius, dir.normalized, raycastBuffer, dir.magnitude + 0.1f, this.mask);
        collisionCount = tmpCount;

        var skip = 0;

        if (tmpCount <= 0) return null;
        if (tmpCount > 0)
        {
            for (int i = 0; i < tmpCount; i++)
            {
                var hitInfo = raycastBuffer[i];
                var canMoveOnSlope = Vector3.Dot(hitInfo.normal.normalized, Vector3.up) > minSlopeDot;
                if(Vector3.Dot(hitInfo.normal, dir.normalized) > 0) Debug.Log("Damn!  " + (dir, hitInfo.normal, hitInfo.collider.gameObject));
                if (hitInfo.normal.normalized == -dir.normalized && hitInfo.distance == 0){
                    collisionCount--;
                    skip++;
                    continue;
                };
                var normal = canMoveOnSlope ?
                    hitInfo.normal.normalized :
                    hitInfo.normal.Set(y: 0).normalized;
                hitNormalsBuffer[i - skip] = normal;

                var angle = 90 - Vector3.Angle(Vector3.up, normal);
                var touchPos = start + dir.normalized * hitInfo.distance;
                if (angle > this.slopeAngle)
                {
                    touchGround = true;
                    groundPos = touchPos;
                    Debug.Log((groundPos, dir));
                }
                if(i == 0) firstTouchPos = touchPos;
            }
        }

        return this.hitNormalsBuffer;
    }

    public Vector3 GetMoveVector(Vector3 center, Vector3 velocity, out Vector3 groundPos)
    {
        groundPos = center;
        var castPoint = center + VectorUtils.Multiply(transform.localScale, groundCheckPoint.localPosition);
        var check = this.Player.CurrentPhysicsScene.Raycast(castPoint, Vector3.down, out var hitInfo, this.distToGround, this.groundMask);
        if(check){
            var tangent = Vector3.Cross(hitInfo.normal, velocity);
            var vel = Vector3.Cross(tangent, hitInfo.normal);
            groundPos = center + Vector3.down * hitInfo.distance;
            return vel.normalized * velocity.magnitude;
        }     
        return velocity;
    }

    public bool IsGrounded(Vector3 center){
        var castPoint = this.GetCastPoint(center);
        var check = this.Player.CurrentPhysicsScene.Raycast(castPoint, Vector3.down, out var hitInfo, this.distToGround, this.groundMask);
        return check;
    }

    private Vector3 GetCastPoint(Vector3 center){
        var result = center + VectorUtils.Multiply(transform.localScale, groundCheckPoint.localPosition) + Vector3.up * 0.2f;
        return result;
    }
}


