using System;
using UnityEngine;
using System.Collections;

namespace Kigor.Rigging
{
    public class CustomTwoBonesIK : RiggingConstraint
    {
        [SerializeField] private Transform root, arm, hand;
        [SerializeField] private Transform target, hint;

        [SerializeField] private float[] lengths, sqrLengths;
        [SerializeField] private Vector3[] positions;
        private Vector3[] rotations;

        private float totalSqrLength;
        private float totalLength;

        private void Awake()
        {
            this.lengths = new float[2];
            this.sqrLengths = new float[2];
            this.positions = new Vector3[2];
            this.rotations = new Vector3[2];

            this.lengths[0] = Vector3.Distance(this.root.position, this.arm.position);
            this.lengths[1] = Vector3.Distance(this.hand.position, this.arm.position);

            this.sqrLengths[0] = this.lengths[0] * this.lengths[0];
            this.sqrLengths[1] = this.lengths[1] * this.lengths[1];

            this.positions[0] = this.arm.position;
            this.positions[1] = this.hand.position;

            this.rotations[0] = this.arm.eulerAngles;
            this.rotations[1] = this.hand.eulerAngles;

            this.totalSqrLength = this.lengths[0] * this.lengths[0] + this.lengths[1] * this.lengths[1];
            this.totalLength = this.lengths[0] + this.lengths[1];
        }

        private void LateUpdate()
        {
            this.SolveIK();
        }

        public override void SolveIK()
        {
            if (!this.target || !this.hint) return;
            var dirToTarget = this.target.position - this.root.position;
            var sqrDistToTarget = dirToTarget.sqrMagnitude;
            var distToTarget = Mathf.Sqrt(sqrDistToTarget);
            dirToTarget.Normalize();
            if (distToTarget >= this.totalLength)
            {
                this.positions[0] = this.root.position + dirToTarget * this.lengths[0];
                this.positions[1] = this.arm.position + dirToTarget * this.lengths[1];
            }
            else
            {
                var cosAngle = (sqrDistToTarget + this.sqrLengths[0] - this.sqrLengths[1]) /
                               (2 * distToTarget * this.lengths[0]);
                var firstEdge = this.lengths[0] * cosAngle;
                var projection = root.position + dirToTarget * firstEdge;
                var height = Mathf.Sqrt(this.sqrLengths[0] - Mathf.Pow(firstEdge, 2));

                var cross = Vector3.Cross(dirToTarget, this.hint.position - projection);
                var midJointDir = Vector3.Cross(cross, dirToTarget).normalized;

                Debug.Log((cosAngle, projection, height));

                this.positions[0] = projection + midJointDir * height;
                this.positions[1] = this.root.position + dirToTarget * distToTarget;
            }

            this.ResolveRig();
        }

        private void ResolveRig()
        {
            // Rotate root
            this.root.rotation = Quaternion.FromToRotation((this.arm.position - this.root.position).normalized,
                (this.positions[0] - this.root.position).normalized) * this.root.rotation;
            //this.arm.transform.position = this.positions[0];

            this.arm.rotation = Quaternion.FromToRotation((this.hand.position - this.arm.position).normalized,
                (this.positions[1] - this.arm.position).normalized) * this.arm.rotation;
            this.hand.transform.position = this.positions[1];
            this.hand.rotation = this.target.rotation;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if(this.positions == null) return;
            Gizmos.DrawWireSphere(this.positions[0], 0.1f);
            Gizmos.DrawWireSphere(this.positions[1], 0.1f);
        }

        

#endif
    }
}