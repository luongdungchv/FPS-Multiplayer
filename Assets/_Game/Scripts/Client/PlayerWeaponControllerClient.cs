using System;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController
    {
#if CLIENT_BUILD
        private float timeCounter;
        [SerializeField] private GameObject test;
        private ClientRecoilManager recoilManager => this.GetComponent<ClientRecoilManager>();
        public WeaponData CurrentWeaponData => this.currentWeapon.Data;
        private ClientBulletTraceManager traceManager => this.GetComponent<ClientBulletTraceManager>();
        
        private void Start()
        {
            Debug.Log(";kfdgljdfglhkdflgh");
            NetworkHandleClient.Instance.OnWeaponChange += this.HandleWeaponChangeMsg;
        }
        #region FSM_CALLBACK

        private partial void NormalStateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                this.currentWeapon.Reload(() => this.FSM.ChangeState(SimpleFSM.StateEnum.Normal));
                this.FSM.ChangeState(SimpleFSM.StateEnum.Reloading);
            }

            if (Input.GetMouseButtonDown(0))
            {
                this.FSM.ChangeState(SimpleFSM.StateEnum.Shooting);
                return;
            }

            this.HandleChangeGun();
        }

        private void HandleChangeGun()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("asdhfas " + this.Player.PlayerID);
                this.SwitchWeapon(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                this.SwitchWeapon(1);
            }
        }

        private partial void ShootStateEnter()
        {
            Debug.Log("Start Shooting");
            this.ShootStateUpdate();
        }

        private partial void ShootStateUpdate()
        {
            Debug.Log("Shoot update");
            if (Input.GetMouseButtonUp(0))
            {
                this.timeCounter = 0;
                this.FSM.ChangeState(SimpleFSM.StateEnum.Normal);
                this.recoilManager.StopRecoil();    
                return;
            }

            if (Input.GetMouseButton(0))
            {
                if (this.timeCounter == 0)
                {
                    var camDir = NetworkCamera.Instance.transform.forward;
                    var headDir = this.GetComponent<PlayerAvatar>().HeadTransform.forward;
                    var shootDir = camDir * Vector3.Dot(camDir, headDir) * 2 - headDir;
                    shootDir.Normalize();
                    
                    this.SendShootPacket(shootDir);
                    this.currentWeapon.PerformShoot();
                    this.recoilManager.ApplyRecoil();

                    var physicsScene = this.Player.CurrentPhysicsScene;
                    var shootPos = this.currentWeapon.ShootPosition;
                    var isHit = physicsScene.Raycast(NetworkCamera.Instance.transform.position, shootDir, out var hitInfo, 100,
                        this.shootMask);
                    Debug.Log(Vector3.Angle(shootDir, camDir));
                    if (isHit)
                    {
                        this.traceManager.ShowTrace(shootPos, hitInfo.point);
                        //this.test.transform.position = hitInfo.point;
                    }
                    else
                    {
                        this.traceManager.ShowTrace(shootPos, shootPos + shootDir * 100);
                    }
                }

                this.timeCounter += Time.deltaTime;
                if (this.timeCounter > this.currentWeapon.Data.shootInterval) this.timeCounter = 0;
            }
        }

        private partial void ReloadStateEnter()
        {
            Debug.Log("Start Reloading");
            this.timeCounter = 0;
        }

        private partial void ReloadStateUpdate()
        {
        }

        #endregion

        public partial void ChangeWeapon(WeaponEnum weapon)
        {
            Debug.Log("Weapon changed to: " + weapon);
            if (weapon == WeaponEnum.None) return;
            if (this.currentWeaponEnum != WeaponEnum.None)
            {
                currentWeapon.gameObject.SetActive(false);
            }

            Debug.Log(this.weaponMap);
            this.currentWeaponEnum = weapon;
            currentWeapon.gameObject.SetActive(true);
            if(this.Player.IsLocalPlayer) this.SendWeaponChangePacket(this.currentWeaponEnum);
        }

        private void HandleWeaponChangeMsg(int playerID, int weapon)
        {
            Debug.Log((playerID, weapon));
            ThreadManager.ExecuteOnMainThread(() =>
            {
                if (this.Player.IsLocalPlayer) return;
                if (playerID != this.Player.PlayerID) return;
                this.ChangeWeapon((WeaponEnum)weapon);
            });
        }

        public partial void SwitchWeapon(int weaponIndex)
        {
            this.ChangeWeapon(this.equippedWeapons[weaponIndex]);
        }

        private void SendShootPacket(Vector3 shootDir)
        {
            var packet = new FPSShootPacket();
            packet.shootDir = shootDir;
            packet.damage = (byte)this.currentWeapon.Data.damage;
            NetworkTransport.Instance.SendPacketTCP(packet);
            Debug.Log("Shoot command sent");
        }

        private void SendWeaponChangePacket(WeaponEnum weapon)
        {
            var packet = new FPSWeaponChangePacket();
            packet.weapon = weapon;
            packet.playerID = (byte)this.Player.PlayerID;
            Debug.Log($"Sent: {packet.playerID} {packet.weapon}");
            NetworkTransport.Instance.SendPacketTCP(packet);
        }

        private void OnDestroy()
        {
            NetworkHandleClient.Instance.OnWeaponChange -= this.HandleWeaponChangeMsg;
        }
#endif
    }
}