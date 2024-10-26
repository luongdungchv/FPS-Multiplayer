using UnityEngine;

namespace Kigor.Networking
{
    public class NetworkPlayerCollider : MonoBehaviour
    {
        [SerializeField] private NetworkFPSPlayer player;

        public NetworkFPSPlayer OwnerPlayer => this.player;
    }
}