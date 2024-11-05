﻿using UnityEngine;

namespace Kigor.Networking
{
    public partial class PlayerHPManager
    {
#if CLIENT_BUILD
        public partial void TakeDamage(int damage)
        {
            this.currentHP -= damage;
            if (this.currentHP <= 0)
            {
                this.Perish();
            }
        }

        public partial void Perish()
        {
            this.gameObject.SetActive(false);
        }
#endif
    }
}