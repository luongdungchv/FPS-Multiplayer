using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCamera : MonoBehaviour
{
    public static NetworkCamera Instance;
    protected virtual void Awake(){
        Instance = this;
#if SERVER_BUILD
        gameObject.SetActive(false);
#endif
    }
}
