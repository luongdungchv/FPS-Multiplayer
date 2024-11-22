using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ThreadManager
{
    private static List<Action> toBeExecutedOnMainThreadsUpdate = new List<Action>();
    private static List<Action> toBeExecutedOnMainThreadsFixedUpdate = new List<Action>();
    private static List<Action> toBeExecutedOnMainThreadsLateUpdate = new List<Action>();
    private static bool updateExec = false;
    private static bool fixedUpdateExec = false;
    private static bool lateUpdateExec = false;

    public static void ExecuteOnMainThread(Action action, ExecuteFunction executeFunction = ExecuteFunction.Update)
    {
        if (action == null)
        {
            return;
        }
        if (executeFunction == ExecuteFunction.Update)
        {
            lock (toBeExecutedOnMainThreadsUpdate)
            {
                toBeExecutedOnMainThreadsUpdate.Add(action);
                updateExec = true;
            }
        }
        else if (executeFunction == ExecuteFunction.FixedUpdate)
        {
            lock (toBeExecutedOnMainThreadsFixedUpdate)
            {
                toBeExecutedOnMainThreadsFixedUpdate.Add(action);
                fixedUpdateExec = true;
            }
        }
        else if (executeFunction == ExecuteFunction.LateUpdate)
        {
            lock (toBeExecutedOnMainThreadsLateUpdate)
            {
                toBeExecutedOnMainThreadsLateUpdate.Add(action);
                lateUpdateExec = true;
            }
        }
    }
    public static void Update()
    {
        if (updateExec)
        {
            Debug.Log(toBeExecutedOnMainThreadsUpdate.Count);
            lock (toBeExecutedOnMainThreadsUpdate)
            {
                try
                {
                    foreach (var i in toBeExecutedOnMainThreadsUpdate)
                    {
                        i.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                updateExec = false;
                toBeExecutedOnMainThreadsUpdate.Clear();
            }
        }
    }

    public static void FixedUpdate()
    {
        if (fixedUpdateExec)
        {
            lock (toBeExecutedOnMainThreadsFixedUpdate)
            {
                try
                {
                    foreach (var i in toBeExecutedOnMainThreadsFixedUpdate)
                    {
                        i.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                fixedUpdateExec = false;
                toBeExecutedOnMainThreadsFixedUpdate.Clear();
            }
        }
    }

    public static void LateUpdate()
    {
        if (lateUpdateExec)
        {
            lock (toBeExecutedOnMainThreadsLateUpdate)
            {
                try
                {
                    foreach (var i in toBeExecutedOnMainThreadsLateUpdate)
                    {
                        i.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                lateUpdateExec = false;
                toBeExecutedOnMainThreadsLateUpdate.Clear();
            }
        }
    }
}
public enum ExecuteFunction
{
    FixedUpdate, Update, LateUpdate
}
