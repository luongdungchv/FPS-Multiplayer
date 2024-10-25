using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController
    {
#if CLIENT_BUILD
        private Weapon currentWeapon => this.weaponMap[this.currentWeaponEnum];
        public partial void ChangeWeapon(WeaponEnum weapon)
        {
            if(this.currentWeaponEnum == WeaponEnum.None) return;
            currentWeapon.gameObject.SetActive(false);
            this.currentWeaponEnum = weapon;
            currentWeapon.gameObject.SetActive(true);
        }

        public partial void HandleInput(FPSInputPacket packet)
        {
            
        }

        private void SendShootPacket()
        {
            var packet = new FPSShootPacket();
            packet.shootDir = NetworkCamera.Instance.transform.forward;
            NetworkTransport.Instance.SendPacketTCP(packet);
        }

        private void SendChangeWeaponPacket(WeaponEnum weapon)
        {
            
        }
#endif
    }
}