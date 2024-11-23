using Kigor.Networking;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController
    {
#if SERVER_BUILD
        public void HandleShootPacket(FPSShootPacket packet)
        {
            Debug.Log("Shoot packet received from player: " + this.Player.PlayerID);

            var currentRule = this.Player.Room.Rule as IPlayersHaveState;
            var diff = this.Player.TickScheduler.CurrentTick - this.Player.CurrentClientTick;
            if (Mathf.Abs(diff) > 75)
            {
                var smaller = Mathf.Min(this.Player.TickScheduler.CurrentTick, this.Player.CurrentClientTick);
                var bigger = Mathf.Max(this.Player.TickScheduler.CurrentTick, this.Player.CurrentClientTick);
                diff = smaller + (255 - bigger);
            }
            currentRule.RevertAllPlayerStates(diff, this.Player);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                this.GetComponent<PlayerAnimationController>().ManuallyUpdateIK();
                Physics.SyncTransforms();
                var dir = packet.shootDir;
                var physicsScene = this.Player.CurrentPhysicsScene;
                var shootPos = this.GetComponent<PlayerAvatar>().HeadTransform.position;
                var check = physicsScene.Raycast(shootPos, dir, out var hitInfo, 100, this.shootMask);
                if (check)
                {
                    var collider = hitInfo.collider.GetComponent<NetworkPlayerCollider>();
                    if (collider)
                    {
                        var playerID = collider.OwnerPlayer.PlayerID;
                        collider.TakeDamage(packet.damage);
                        this.SendPlayerShotPacket(playerID, hitInfo.point);
                    }

                    this.SendShotRespondPacket(this.Player.PlayerID, hitInfo.point);
                }
                else
                {
                    var endPos = shootPos + dir * 100;
                    this.SendShotRespondPacket(this.Player.PlayerID, endPos);
                }

                currentRule.RestoreAllPlayerStates();
            }, ExecuteFunction.LateUpdate);
        }

        public void HandleReloadPacket(FPSWeaponReloadPacket packet)
        {
            var currentSec = (byte)System.DateTime.UtcNow.Second;
            var currentMilSec = System.DateTime.UtcNow.Millisecond;
            var duration = packet.duration;
            Debug.Log((duration, packet.sendTimeMili, packet.sendTimeSec, currentSec, currentMilSec));
            // if (currentSec < packet.sendTimeSec) duration += (float)currentMilSec / 1000 + (60 - (float)packet.sendTimeMili / 1000);
            // else duration += (float)(currentMilSec - packet.sendTimeMili) / 1000;
            this.currentWeapon.Reload(duration);
        }

        public void HandleChangeWeaponPacket(FPSWeaponChangePacket packet)
        {
            this.ChangeWeapon(packet.weapon);
        }

        private void BroadcastReloadPacket(float duration, int sendTimeMili, int sendTimeSec)
        {
            var packet = new FPSWeaponReloadPacket();
            packet.playerID = (byte)this.Player.PlayerID;
            packet.duration = duration;
            packet.sendTimeMili = (ushort)sendTimeMili;
            packet.sendTimeSec = (byte)sendTimeSec;

            var msg = packet.EncodeData();
            this.Player.Room.BroadcastMessage(msg);
        }

        private void SendPlayerShotPacket(int playerID, Vector3 hitPos)
        {
            var packet = new FPSPlayerShotPacket();
            packet.playerID = (byte)playerID;
            packet.hitPos = hitPos;

            var msg = packet.EncodeData();
            this.Player.Socket.SendDataTCP(msg);
        }

        private void SendShotRespondPacket(int playerID, Vector3 endPos)
        {
            var resPacket = new FPSServerRespondShotPacket();
            resPacket.playerID = (byte)playerID;
            resPacket.endPos = endPos;
            var msg = resPacket.EncodeData();
            
            this.Player.Room.BroadcastMessage(msg);
        }

        public partial void ChangeWeapon(WeaponEnum weapon)
        {
            this.currentWeaponEnum = weapon;
        }

        public partial void SwitchWeapon(int weaponIndex)
        {
            this.ChangeWeapon(this.equippedWeapons[weaponIndex]);
        }

        private partial void NormalStateUpdate()
        {
        }

        private partial void ShootStateEnter()
        {
        }

        private partial void ShootStateUpdate()
        {
        }

        private partial void ReloadStateEnter()
        {
        }

        private partial void ReloadStateUpdate()
        {
        }
#endif
    }
}