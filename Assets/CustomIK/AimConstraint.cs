using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimConstraint : MonoBehaviour
{
    [SerializeField] private Transform aimTarget, root;
    
    [SerializeField] private AlignAxis alignAxis, upAxis;


    private void LateUpdate()
    {
        var worldAlignDir = root.TransformDirection(this.GetAxisVector(this.alignAxis));
        var dirToTarget = (this.aimTarget.position - this.root.position);
        dirToTarget.Normalize();

        var rotation = Quaternion.FromToRotation(worldAlignDir, dirToTarget);
        this.root.rotation = rotation * this.root.rotation;
        
        
    }

    private Vector3 GetAxisVector(AlignAxis axis)
    {
        switch (axis)
        {
            case AlignAxis.X:
            {
                return Vector3.right;
            }
            case AlignAxis.MinusX:
            {
                return Vector3.left;
            }
            case AlignAxis.Y:
            {
                return Vector3.up;
            }
            case AlignAxis.MinusY:
            {
                return Vector3.down;
            }
            case AlignAxis.Z:
            {
                return Vector3.forward;
            }
            case AlignAxis.MinusZ:
            {
                return Vector3.back;
            }
            default: return Vector3.zero;
        };
    }
    
}

public enum AlignAxis
{
    X, MinusX, Y, MinusY, Z, MinusZ
}
