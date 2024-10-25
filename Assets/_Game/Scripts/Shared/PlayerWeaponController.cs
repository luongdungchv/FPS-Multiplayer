using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<WeaponEnum, Weapon> weaponMap;

        private WeaponEnum currentWeaponEnum;

        public partial void HandleInput(FPSInputPacket packet);
        

        public partial void ChangeWeapon(WeaponEnum weapon);

    }

    public enum WeaponEnum
    {
        None, AK47, M4A1, USP, Glock
    }
}