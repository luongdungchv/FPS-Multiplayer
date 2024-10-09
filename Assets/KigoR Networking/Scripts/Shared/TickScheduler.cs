using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kigor.Networking;
using UnityEngine;
using UnityEngine.Events;

public class TickScheduler : MonoBehaviour
{
    public static int MAX_TICK = 256;

    private UnityAction OnTickProcessed;
    [SerializeField] private float tickRate = 33;

    private float tickInterval;
    private float timeCounter;
    private int currentTick;

    public int CurrentTick => this.currentTick;
    public float TickDeltaTime => 1 / this.tickRate;

    private void Awake()
    {
    }

    private void Start()
    {
        this.tickInterval = 1 / tickRate;

    }

    public void RegisterTickCallback(UnityAction callback)
    {
        OnTickProcessed += callback;
    }
    public void UnregisterTickCallback(UnityAction callback)
    {
        OnTickProcessed -= callback;
    }
    public void SetTick(int tick) => this.currentTick = tick;


    private void Update()
    {
        timeCounter += Time.deltaTime;
        if (timeCounter > tickInterval)
        {
            var tickCount = timeCounter / tickInterval;
            for (int i = 0; i < tickCount; i++)
            {
                currentTick++;
                if(currentTick >= MAX_TICK) currentTick = 1;
                this.OnTickProcessed?.Invoke();
            }
            this.timeCounter = this.timeCounter % tickInterval;
        }
    }
}
