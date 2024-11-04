using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<WeaponEnum, Weapon> weaponMap;
        [SerializeField] private LayerMask shootMask;
        private WeaponEnum currentWeaponEnum;

        private NetworkFPSPlayer Player => this.GetComponent<NetworkFPSPlayer>();
        private Weapon currentWeapon => this.weaponMap[this.currentWeaponEnum];
        public SimpleFSM FSM => this.GetComponent<SimpleFSM>();

        private void Awake()
        {
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

    }

    public enum WeaponEnum
    {
        None, AK47, M4A1, USP, Glock
    }
}