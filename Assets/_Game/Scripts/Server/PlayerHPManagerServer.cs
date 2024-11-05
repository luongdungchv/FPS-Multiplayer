using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerHPManager
    {
#if SERVER_BUILD
        public partial void TakeDamage(int damage)
        {
            this.currentHP -= damage;
            if (this.currentHP <= 0)
            {
                this.Perish();
            }
        }

        public partial void Perish()
        {
            this.gameObject.SetActive(false);
            this.isDead = true;
            this.SendPlayerDiePacket();
        }

        private void SendPlayerDiePacket()
        {
            var packet = new FPSPlayerDiePacket();
            packet.playerID = (byte)this.Player.PlayerID;
            this.Player.Socket.SendDataTCP(packet.EncodeData());
        }
#endif
    }
}