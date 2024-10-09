using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class NetworkWaitingPlayer
    {
        private string playerName;
        
        public string PlayerName => this.playerName;
#if CLIENT_BUILD
        public NetworkWaitingPlayer(string name){
            this.playerName = name;
        }
#endif
        
    }
}
