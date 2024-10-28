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
            this.isReloading = true;
            DL.Utils.CoroutineUtils.Invoke(this, () => this.isReloading = false, duration);
        }
#endif
    }
}