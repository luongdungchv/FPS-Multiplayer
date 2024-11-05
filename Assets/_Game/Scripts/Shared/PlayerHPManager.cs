using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerHPManager : MonoBehaviour
    {
        [SerializeField] private int maxHP;
        [SerializeField] private List<NetworkPlayerCollider> hitBoxes;
        private int currentHP;

        private NetworkFPSPlayer Player;

        private void Awake() 
        {
            this.currentHP = this.maxHP;
            this.Player = this.GetComponent<NetworkFPSPlayer>();
            
            this.hitBoxes.ForEach(x => x.SetOwnerPlayer(this.Player));
            this.hitBoxes.ForEach(x => x.SetHpManager(this));
        }

        public partial void TakeDamage(int damage);
        public partial void Perish();
    }
}