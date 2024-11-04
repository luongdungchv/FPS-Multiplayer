using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<WeaponEnum, Weapon> localWeaponMap, otherWeaponMap;
        [SerializeField] private LayerMask shootMask;
        [SerializeField] private WeaponEnum currentWeaponEnum;

        private WeaponEnum[] equippedWeapons;
        
        private NetworkFPSPlayer Player => this.GetComponent<NetworkFPSPlayer>();
        private Weapon currentWeapon => this.weaponMap[this.currentWeaponEnum];
        public SimpleFSM FSM => this.GetComponent<SimpleFSM>();
        private Dictionary<WeaponEnum, Weapon> weaponMap => this.Player.IsLocalPlayer ? localWeaponMap : otherWeaponMap;

        private void Awake()
        {
            this.equippedWeapons = new WeaponEnum[2];

            this.equippedWeapons[0] = WeaponEnum.AK47;
            this.equippedWeapons[1] = WeaponEnum.USP;
            
            if (this.weaponMap == null) return;
            foreach (var pair in this.weaponMap)
            {
                Debug.Log(pair);
                pair.Value.SetOwner(this.Player);
            }

            this.FSM.GetState(SimpleFSM.StateEnum.Normal).OnStateUpdate.AddListener(this.NormalStateUpdate);
            this.FSM.GetState(SimpleFSM.StateEnum.Shooting).OnStateEnter.AddListener(this.ShootStateEnter);
            this.FSM.GetState(SimpleFSM.StateEnum.Shooting).OnStateUpdate.AddListener(this.ShootStateUpdate);
            this.FSM.GetState(SimpleFSM.StateEnum.Reloading).OnStateEnter.AddListener(this.ReloadStateEnter);
            this.FSM.GetState(SimpleFSM.StateEnum.Reloading).OnStateUpdate.AddListener(this.ReloadStateUpdate);
        }

        private partial void NormalStateUpdate();
        private partial void ShootStateEnter();
        private partial void ShootStateUpdate();
        private partial void ReloadStateEnter();
        private partial void ReloadStateUpdate();


        public partial void ChangeWeapon(WeaponEnum weapon);
        public partial void SwitchWeapon(int weaponIndex);

    }

    public enum WeaponEnum
    {
        None, AK47, M4A1, USP, Glock
    }
}