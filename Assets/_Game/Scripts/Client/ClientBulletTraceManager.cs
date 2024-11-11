using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Kigor.Networking
{
    public class ClientBulletTraceManager : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
        [SerializeField] private Queue<GameObject> tracesQueue;
        [SerializeField] private Transform traceContainer;
        [SerializeField] private float traceSpeed;

        private void Awake()
        {
            // foreach (var trace in this.tracesQueue)
            // {
            //     trace.GetComponent<TrailRenderer>().enabled = false;
            // }
        }
        
        public void ShowTrace(Vector3 startPos, Vector3 endPos)
        {
            if (this.tracesQueue.Count > 0)
            {
                var trace = this.tracesQueue.Dequeue();
                trace.GetComponent<TrailRenderer>().enabled = true;
                
                trace.transform.position = startPos;
                trace.transform.SetParent(null);
                
                var time = Vector3.Distance(startPos, endPos) / traceSpeed;
                DL.Utils.CoroutineUtils.Invoke(this, () =>
                {
                    trace.gameObject.SetActive(true);
                    trace.transform.DOMove(endPos, time).OnComplete((() =>
                    {
                        trace.gameObject.SetActive(false);
                        //trace.transform.position = this.GetComponent<PlayerWeaponController>();
                        trace.transform.SetParent(this.traceContainer);
                        trace.transform.localPosition = Vector3.zero;
                        tracesQueue.Enqueue(trace);
                    }));
                }, 0);
                // trace.transform.DOMove(endPos, time).OnComplete((() =>
                // {
                //     trace.gameObject.SetActive(false);
                //     //trace.transform.position = this.GetComponent<PlayerWeaponController>();
                //     trace.transform.SetParent(this.traceContainer);
                //     trace.transform.localPosition = Vector3.zero;
                //     tracesQueue.Enqueue(trace);
                // }));
            }
        }
    }
}