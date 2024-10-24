using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhysicsCast : MonoBehaviour
{
    private BoxCollider boxCollider => this.GetComponent<BoxCollider>();
    private CapsuleCollider capsuleCollider => this.GetComponent<CapsuleCollider>();

    private RaycastHit[] raycastBuffer;


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

}
