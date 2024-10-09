using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManagerHolder : MonoBehaviour
{
    private void Update()
    {
        ThreadManager.Update();
    }
    private void FixedUpdate() => ThreadManager.FixedUpdate();
    private void LateUpdate() => ThreadManager.LateUpdate();
}
