using UnityEngine;

namespace Kigor.Networking
{
    public partial class Weapon
    {
#if CLIENT_BUILD
        private int currentAmmo;
        private int currentReservedAmmo;

        public int CurrentAmmo => this.currentAmmo;
        protected partial void Awake()
        {
            this.currentAmmo = this.data.magazineSize;
            this.currentReservedAmmo = this.currentAmmo * 3;
        }

        public void Reload()
        {
            Debug.Log("start reloading");
            if (this.currentReservedAmmo == 0) return;
            this.isReloading = true;
            DL.Utils.CoroutineUtils.Invoke(this, () =>
            {
                var oldAmmo = this.currentAmmo;
                this.isReloading = false;
                var filledAmmo = this.currentAmmo - oldAmmo;
                this.currentReservedAmmo -= filledAmmo;
                this.currentAmmo = this.currentReservedAmmo < 0 ? -this.currentReservedAmmo : this.data.magazineSize;
            }, this.data.reloadDuration);
            this.SendReloadMsgToServer();
        }

        public void PerformShoot()
        {
            this.currentAmmo--;
            if (this.currentAmmo == 0)
            {
                this.Reload();
            }
        }

        private void SendReloadMsgToServer()
        {
            var packet = new FPSWeaponReloadPacket();
            packet.duration = this.data.reloadDuration;
            packet.sendTimeMili = (ushort)System.DateTime.UtcNow.Millisecond;
            packet.sendTimeSec = (byte)System.DateTime.UtcNow.Second;
            Debug.Log((packet.sendTimeSec, packet.sendTimeMili));
            NetworkTransport.Instance.SendPacketTCP(packet);
        }
#endif
    }
}