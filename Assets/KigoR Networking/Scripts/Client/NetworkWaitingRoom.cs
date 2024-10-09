using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class NetworkWaitingRoom
    {
        private int roomID;

        public int RoomID => this.roomID;
        private List<NetworkWaitingPlayer> waitingPlayerList;

        public void SetRoomID(int id)
        {
            this.roomID = id;
        }
#if CLIENT_BUILD
        private NetworkWaitingPlayer localPlayer;
        public void AddPlayer(NetworkWaitingPlayer player, bool localPlayer = false){
            this.waitingPlayerList.Add(player);
            if(localPlayer) this.localPlayer = player;
        }
        public void RemovePlayer(NetworkWaitingPlayer player){
            this.waitingPlayerList.Remove(player);
        }
#endif
    }
    
    public enum WaitingRoomCommand{
        
    }
}
