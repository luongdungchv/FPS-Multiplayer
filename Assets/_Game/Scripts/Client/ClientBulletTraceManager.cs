using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Kigor.Networking
{
    public class ClientBulletTraceManager : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
        [SerializeField] private Queue<GameObject> tracesQueue;
        [SerializeField] private Queue<ParticleSystem> hitFXQueue;
        [SerializeField] private Transform traceContainer;
        [SerializeField] private float traceSpeed;
        
        
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
                        trace.transform.SetParent(this.traceContainer);
                        trace.transform.localPosition = Vector3.zero;
                        tracesQueue.Enqueue(trace);
                    }));
                }, 0);

            }
        }

        public void ShowHitFX(Vector3 position, Vector3 lookDir)
        {
            if (this.hitFXQueue.Count > 0)
            {
                var fx = this.hitFXQueue.Dequeue();
                fx.gameObject.SetActive(true);
                fx.transform.SetParent(null);

                fx.transform.position = position + lookDir.normalized * 0.01f;
                fx.transform.rotation = Quaternion.LookRotation(lookDir);
                
                fx.Play();
                var duration = fx.main.duration;
                DL.Utils.CoroutineUtils.Invoke(this, () =>
                {
                    fx.gameObject.SetActive(false);
                    this.hitFXQueue.Enqueue(fx);
                }, duration);
            }
        }
    }
}