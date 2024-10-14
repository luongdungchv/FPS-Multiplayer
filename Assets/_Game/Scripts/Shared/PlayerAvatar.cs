using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour
{
    [SerializeField] private Transform headTransform;

    public Vector3 GetHeadPosition(Vector3 parentPos){
        return parentPos + VectorUtils.Multiply(headTransform.localPosition, headTransform.localScale);
    }

    public Transform HeadTransform => this.headTransform;
}
