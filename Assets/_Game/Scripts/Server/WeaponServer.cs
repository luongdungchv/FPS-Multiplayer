using UnityEngine;

namespace Kigor.Networking
{
    public partial class Weapon
    {
#if SERVER_BUILD
        protected partial void Awake()
        {
            
        }

        public void Reload(float duration)
        {
            Debug.Log($"Reload received: duration: {duration}");
            //this.isReloading = true;
            //DL.Utils.CoroutineUtils.Invoke(this, () => this.isReloading = false, duration);
        }
#endif
    }
}