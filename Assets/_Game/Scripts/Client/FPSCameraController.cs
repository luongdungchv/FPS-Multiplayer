using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;

public class FPSCameraController : NetworkCamera
{
    [SerializeField] private Vector2 threshold;

    public void PerformVerticalRotation(float amount, float sensitivity){
        var currentRot = transform.localEulerAngles;
        var mouseY = amount * sensitivity;
        Debug.Log(mouseY);

        if(currentRot.x > 180)
            currentRot.x -= 360;
        currentRot.x -= mouseY;
        currentRot.x = Mathf.Clamp(currentRot.x, threshold.x, threshold.y);

        transform.localEulerAngles = currentRot;
    }
}
