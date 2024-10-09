using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using K4os.Compression.LZ4;
using System.Reflection;
using Kigor.Networking;
using System;
using UnityEngine.Events;

public class TestCompress : MonoBehaviour
{
    private TestClass test;
    private UnityAction<TestClass> OnTest;
    private void Awake() {
        this.test = new TestClass();
    }
    private void Start(){
        //test = null;
        GC.Collect();
    }
    public void TestMethod(){
        var test = new TestClass();
    }
    private void Test(){
        var assem = Assembly.GetAssembly(typeof(NetworkGameRule));
        var types = assem.GetTypes();
    }

    public class TestClass{
        ~TestClass(){
            Debug.Log("test");
        }
    }
}
