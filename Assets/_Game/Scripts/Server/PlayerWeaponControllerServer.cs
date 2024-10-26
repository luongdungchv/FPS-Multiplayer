using Kigor.Networking;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController
    {
#if SERVER_BUILD
        public partial void HandleInput(FPSInputPacket packet)
        {
            
        }

        public void HandleShootPacket(FPSShootPacket packet)
        {
            Debug.Log("Shoot packet received from player: " + this.Player.PlayerID);
            var dir = packet.shootDir;
            var physicsScene = this.Player.CurrentPhysicsScene;
            var shootPos = NetworkCamera.Instance.transform.position;
            ThreadManager.ExecuteOnMainThread(() =>
            {
                var check = physicsScene.Raycast(shootPos, dir, out var hitInfo, 100, this.shootMask);
                if (check)
                {
                    // TODO: Damage dealing
                    var collider = hitInfo.collider.GetComponent<NetworkPlayerCollider>();
                    if (!collider) return;
                    var playerID = collider.OwnerPlayer.PlayerID;
                    this.SendPlayerShotPacket(playerID, hitInfo.point);
                }
            });
        }

        private void SendPlayerShotPacket(int playerID, Vector3 hitPos)
        {
            var packet = new FPSPlayerShotPacket();
            packet.playerID = (byte)playerID;
            packet.hitPos = hitPos;

            var msg = packet.EncodeData();
            this.Player.Socket.SendDataTCP(msg);
        }

        public partial void ChangeWeapon(WeaponEnum weapon)
        {
            
        }
#endif
    }
}