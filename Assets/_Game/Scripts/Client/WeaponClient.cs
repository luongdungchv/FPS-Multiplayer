using UnityEngine;
using UnityEngine.Events;

namespace Kigor.Networking
{
    public partial class Weapon
    {
#if CLIENT_BUILD
        [SerializeField] private int currentAmmo;
        private int currentReservedAmmo;

        private ClientMuzzleFlashManager muzzleFlashManager => this.GetComponent<ClientMuzzleFlashManager>();
        public int CurrentAmmo => this.currentAmmo;

        protected partial void Awake()
        {
        }

        private void Start()
        {
            if (!this.owner.IsLocalPlayer) return;
            this.currentAmmo = this.data.magazineSize;
            this.currentReservedAmmo = this.currentAmmo * 3;
            this.data.ConstructRecoilDirections();
        }

        public void Reload(UnityAction onComplete = null)
        {
            Debug.Log("start reloading");
            //if (this.currentReservedAmmo == 0) return;
            this.isReloading = true;
            this.owner.GetComponent<ClientRecoilManager>().StopRecoil();
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
            this.muzzleFlashManager.PlayMuzzle();
            if (this.currentAmmo == 0)
            {
                this.Reload(() =>
                    this.owner.GetComponent<PlayerWeaponController>().FSM.ChangeState(SimpleFSM.StateEnum.Normal));
                this.owner.GetComponent<PlayerWeaponController>().FSM.ChangeState(SimpleFSM.StateEnum.Reloading);
            }
        }

        private void SendReloadMsgToServer()
        {
            var packet = new FPSWeaponReloadPacket();
            packet.duration = this.data.reloadDuration;
            packet.sendTimeMili = (ushort)System.DateTime.UtcNow.Millisecond;
            packet.sendTimeSec = (byte)System.DateTime.UtcNow.Second;
            NetworkTransport.Instance.SendPacketTCP(packet);
        }
#endif
    }
}