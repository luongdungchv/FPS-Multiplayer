using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SimpleFSM : Sirenix.OdinInspector.SerializedMonoBehaviour
{
    [SerializeField] private Dictionary<StateEnum, State> statesMap;

    [SerializeField] private State currentState;
    [SerializeField] private StateEnum currentStateEnum;

    private void Start()
    {
        currentState = this.statesMap.ElementAt(0).Value;
        this.currentState.OnStateEnter?.Invoke();
    }

    private void Update()
    {
        if (this.currentState != null)
        {
            Debug.Log(this.currentState.OnStateUpdate);
            this.currentState.OnStateUpdate?.Invoke();
        }
    }
    private void FixedUpdate()
    {
        if (this.currentState != null)
        {
            this.currentState.OnStateFixedUpdate?.Invoke();
        }
    }
    private void LateUpdate()
    {
        if (this.currentState != null)
        {
            this.currentState.OnStateLateUpdate?.Invoke();
        }
    }

    public State GetState(StateEnum stateEnum) => statesMap[stateEnum];

    public void ChangeState(StateEnum destState)
    {
        if (this.currentStateEnum == destState) return;
        this.currentState.OnStateExit?.Invoke();
        this.currentState = this.GetState(destState);
        this.currentState.OnStateEnter?.Invoke();
        this.currentStateEnum = destState;
    }
    
    [System.Serializable]
    public class State
    {
        public string label;
        public UnityEvent OnStateEnter;
        public UnityEvent OnStateExit;
        public UnityEvent OnStateUpdate;
        public UnityEvent OnStateFixedUpdate;
        public UnityEvent OnStateLateUpdate;
    }

    public enum StateEnum
    {
        Normal, Shooting, Reloading
    }
}
