using UnityEngine;

namespace Kigor.Networking
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponEnum weaponEnum;
        [SerializeField] private WeaponData data;
        [SerializeField] private Transform weaponRoot;

        private NetworkPlayer owner;
        
        public void SetOwner(NetworkPlayer owner) => this.owner = owner;
        public WeaponData Data => this.data;
    }
}