using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ThreadManager
{
    private static List<Action> toBeExecutedOnMainThreads = new List<Action>();
    private static bool execute = false;

    public static void ExecuteOnMainThread(Action action)
    {
        if (action == null)
        {
            return;
        }
        lock (toBeExecutedOnMainThreads)
        {
            toBeExecutedOnMainThreads.Add(action);
            execute = true;
        }
    }
    public static void Update()
    {
        if (execute)
        {
            lock (toBeExecutedOnMainThreads)
            {
                try
                {
                    foreach (var i in toBeExecutedOnMainThreads)
                    {
                        i.Invoke();
                    }
                }
                catch (Exception ex){
                    Debug.LogError(ex);
                }
                execute = false;
                toBeExecutedOnMainThreads.Clear();
            }
        }
    }
}
