using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;

using NetworkPlayer = Kigor.Networking.NetworkPlayer;

public class ClientCameraController : NetworkCamera
{
    public static ClientCameraController Instance;
    [SerializeField] private NetworkPlayer target;
    [SerializeField] private Vector2 verticalRotationLimit;

    protected override void Awake(){
        base.Awake();
        Instance = this;
    }

    public void SetTargetPlayer(NetworkPlayer target){
        this.target = target;
    }

    private void Start(){
        this.transform.eulerAngles = Vector3.zero;
    }

    private void LateUpdate() {
        this.PerformRotation();
        transform.position = target.transform.position;
    }

    private void PerformRotation(){
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");

        var currentRot = transform.eulerAngles;
        currentRot.y += mouseX;

        if(currentRot.x > 180)
            currentRot.x -= 360;
        currentRot.x -= mouseY;
        currentRot.x = Mathf.Clamp(currentRot.x, verticalRotationLimit.x, verticalRotationLimit.y);

        transform.eulerAngles = currentRot;
    }
}
