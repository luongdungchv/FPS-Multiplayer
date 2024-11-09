using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestPhysicsCast : MonoBehaviour
{
    private BoxCollider boxCollider => this.GetComponent<BoxCollider>();
    private CapsuleCollider capsuleCollider => this.GetComponent<CapsuleCollider>();

    private RaycastHit[] raycastBuffer;
    [SerializeField] private GameObject testObject;
    [SerializeField] private Transform testPoint;


    private void Awake(){
        raycastBuffer = new RaycastHit[7];
    }

    [Sirenix.OdinInspector.Button]
    private void TestBoxCast(){
        var hitList = Physics.BoxCastAll(transform.position, this.boxCollider.size / 2, transform.forward, Quaternion.identity, 100);
        foreach(var hitInfo in hitList){
            Debug.Log((hitInfo.point, hitInfo.distance));
        }
    }

    [Sirenix.OdinInspector.Button]
    private void TestCapsuleCast(float maxdist = 100){
        var radius = this.capsuleCollider.radius;
        var dist = capsuleCollider.height / 2 - radius;
        var top = transform.position + Vector3.up * dist;
        var bot = transform.position - Vector3.up * dist;

        var hitList = Physics.CapsuleCastAll(top, bot, radius, transform.forward, maxdist);
        foreach(var hitInfo in hitList){
            Debug.Log((hitInfo.point, hitInfo.distance, hitInfo.normal));
        }
    }

    [Sirenix.OdinInspector.Button]
    private void OtherTest()
    {
        var oldPos = this.testObject.transform.position;
        this.testObject.transform.position = this.testPoint.position;
        Physics.SyncTransforms();
        var physicsScene = SceneManager.GetSceneAt(0).GetPhysicsScene();
        var check = physicsScene.Raycast(transform.position, transform.forward, out var hitInfo, 100);
        if (check) Debug.Log(hitInfo.collider.gameObject);
        this.testObject.transform.position = oldPos;
    }

    [Sirenix.OdinInspector.Button]
    private void TestRotation(Vector3 dir)
    {
        var targetDir = transform.TransformDirection(dir);
        var rot = Quaternion.FromToRotation(transform.forward, targetDir);
        transform.rotation = rot;
    }

}
