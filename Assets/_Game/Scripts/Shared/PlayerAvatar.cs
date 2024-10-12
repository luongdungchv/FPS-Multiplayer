using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour
{
    [SerializeField] private Transform headTransform;

    public Transform HeadTransform => this.headTransform;
}
