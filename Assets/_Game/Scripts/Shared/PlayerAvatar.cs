using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAvatar : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform camHolder;

    public Vector3 GetHeadPosition(Vector3 parentPos){
        return parentPos + VectorUtils.Multiply(headTransform.localPosition, headTransform.localScale);
    }

    public Transform HeadTransform => this.headTransform;
    public Transform CamHolder => this.camHolder;
}
