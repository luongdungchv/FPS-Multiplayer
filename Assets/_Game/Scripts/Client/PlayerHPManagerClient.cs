using System;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerHPManager
    {
#if CLIENT_BUILD
        private void Start()
        {
            Debug.Log("Start");
            NetworkHandleClient.Instance.OnPlayerDie += this.HandlePlayerDie;
        }
        public partial void TakeDamage(int damage)
        {
            this.currentHP -= damage;
            if (this.currentHP <= 0)
            {
                this.Perish();
            }
        }

        private void HandlePlayerDie(int playerID)
        {
            Debug.Log($"Player Die: {playerID}");
            if(playerID != this.Player.PlayerID) return;
            ThreadManager.ExecuteOnMainThread(this.Perish);
        }

        public partial void Perish()
        {
            
            this.gameObject.SetActive(false);
            this.isDead = true;
        }

        private void OnDestroy()
        {
            NetworkHandleClient.Instance.OnPlayerDie -= this.HandlePlayerDie;
        }
#endif
    }
}