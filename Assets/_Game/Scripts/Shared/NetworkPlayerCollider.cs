using UnityEngine;

namespace Kigor.Networking
{
    public class NetworkPlayerCollider : MonoBehaviour
    {
        [SerializeField] private NetworkFPSPlayer player;
        [SerializeField] private PlayerHPManager hpManager;
        public NetworkFPSPlayer OwnerPlayer => this.player;

        public void SetOwnerPlayer(NetworkFPSPlayer player) => this.player = player;
        public void SetHpManager(PlayerHPManager hpManager) => this.hpManager = hpManager;

        public void TakeDamage(int inputDamage)
        {
            Debug.Log("take damage");
            this.hpManager.TakeDamage(inputDamage);
        }
    }
}