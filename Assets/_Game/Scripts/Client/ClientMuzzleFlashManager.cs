using UnityEngine;
using System.Collections.Generic;

namespace Kigor.Networking
{
    public class ClientMuzzleFlashManager : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
        [SerializeField] private Queue<ParticleSystem> fxQueue;

        public void PlayMuzzle()
        {
            var fx = this.fxQueue.Dequeue();
            fx.gameObject.SetActive(true);
            var duration = fx.main.duration;
            fx.Play();
            DL.Utils.CoroutineUtils.Invoke(this, () =>
            {
                fx.gameObject.SetActive(false);
                this.fxQueue.Enqueue(fx);
            }, duration);
        }
    }
}