using System;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponEnum weaponEnum;
        [SerializeField] private WeaponData data;
        [SerializeField] private Transform weaponRoot;

        public WeaponData Data => this.data;
        private NetworkPlayer owner;
        [SerializeField] private bool isReloading;
        
        public bool IsReloading => this.isReloading;
        
        public void SetOwner(NetworkPlayer owner) => this.owner = owner;

        protected partial void Awake();
    }
}