using System;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController
    {
#if CLIENT_BUILD
        private float timeCounter;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O)) this.SendShootPacket();

            if (Input.GetMouseButtonUp(0))
            {
                this.timeCounter = 0;
                return;
            }
            if (Input.GetMouseButton(0) && !this.currentWeapon.IsReloading)
            {
                if (this.timeCounter == 0)
                {
                    this.SendShootPacket();
                    this.currentWeapon.PerformShoot();
                }
                
                this.timeCounter += Time.deltaTime;
                if (this.timeCounter > this.currentWeapon.Data.shootInterval) this.timeCounter = 0;
            }
        }

        public partial void ChangeWeapon(WeaponEnum weapon)
        {
            Debug.Log("Weapon changed to: " + weapon);
            if (this.currentWeaponEnum != WeaponEnum.None)
            {
                currentWeapon.gameObject.SetActive(false);
            }
            this.currentWeaponEnum = weapon;
            currentWeapon.gameObject.SetActive(true);
        }

        public partial void HandleInput(FPSInputPacket packet)
        {
            // if (packet.shoot)
            // {
            //     if (this.timeCounter == 0)
            //     {
            //         this.SendShootPacket();
            //     }
            //
            //     this.timeCounter += this.Player.TickScheduler.TickDeltaTime;
            //     if (this.timeCounter > this.currentWeapon.Data.shootInterval) this.timeCounter = 0;
            // }
        }

        private void SendShootPacket()
        {
            var packet = new FPSShootPacket();
            packet.shootDir = NetworkCamera.Instance.transform.forward;
            NetworkTransport.Instance.SendPacketTCP(packet);
            Debug.Log("Shoot command sent");
        }

        private void ProcessClientShoot()
        {
            var shootDir = NetworkCamera.Instance.transform.forward;
            var shootPos = NetworkCamera.Instance.transform.position;
            var physicsScene = this.Player.CurrentPhysicsScene;
            var shootCheck = physicsScene.Raycast(shootPos, shootDir, out var hitInfo, 100, this.shootMask);
            if (shootCheck)
            {
                var collider = hitInfo.collider.GetComponent<NetworkPlayerCollider>();
                if (collider)
                {
                    
                }
            }
        }

        private void SendChangeWeaponPacket(WeaponEnum weapon)
        {
            
        }
#endif
    }
}