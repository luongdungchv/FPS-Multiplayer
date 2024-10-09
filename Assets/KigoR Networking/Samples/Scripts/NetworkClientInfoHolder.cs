using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;

public class NetworkClientInfoHolder : MonoBehaviour
{
    public static NetworkClientInfoHolder Instance;
    public string playerName;

    private void Awake() {
        Instance = this;  
    }
}
