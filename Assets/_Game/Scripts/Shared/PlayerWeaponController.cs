using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<WeaponEnum, Weapon> weaponMap;
        [SerializeField] private LayerMask shootMask;
        [SerializeField] private WeaponEnum currentWeaponEnum;

        private NetworkFPSPlayer Player => this.GetComponent<NetworkFPSPlayer>();
        private Weapon currentWeapon => this.weaponMap[this.currentWeaponEnum];
        
        private void Awake()
        {
            if (this.weaponMap == null) return;
            foreach (var pair in this.weaponMap)
            {
                Debug.Log(pair);
                pair.Value.SetOwner(this.Player);
            }
        }


        public partial void ChangeWeapon(WeaponEnum weapon);

    }

    public enum WeaponEnum
    {
        None, AK47, M4A1, USP, Glock
    }
}