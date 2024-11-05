﻿using UnityEngine;
using UnityEngine.Events;

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
            if (!this.owner.IsLocalPlayer) return;
            this.currentAmmo = this.data.magazineSize;
            this.currentReservedAmmo = this.currentAmmo * 3;
        }

        public void Reload(UnityAction onComplete = null)
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
                onComplete?.Invoke();
            }, this.data.reloadDuration);
            this.SendReloadMsgToServer();
        }

        public void PerformShoot()
        {
            this.currentAmmo--;
            if (this.currentAmmo == 0)
            {
                this.Reload(() => this.owner.GetComponent<PlayerWeaponController>().FSM.ChangeState(SimpleFSM.StateEnum.Normal));
                this.owner.GetComponent<PlayerWeaponController>().FSM.ChangeState(SimpleFSM.StateEnum.Reloading);
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