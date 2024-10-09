using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Kigor.Networking
{
    public class NetworkWaitingRoomManager
    {
#if SERVER_BUILD
        private NetworkWaitingRoom[] waitingRoomList;
        private NetworkPortCluster cluster;
        
        private const int MAX_ROOM = 256;
        
        public NetworkWaitingRoomManager(NetworkPortCluster ownerCluster){
            this.cluster = ownerCluster;
            this.waitingRoomList = new NetworkWaitingRoom[MAX_ROOM];
        }
        
        public NetworkWaitingRoom AddWaitingRoom(){
            var waitingRoom = this.cluster.CreateWaitingRoom();
            
            for(int i = 0; i < MAX_ROOM; i++){
                if(waitingRoomList[i] == null)
                {
                    waitingRoom.SetRoomID(i);
                    waitingRoomList[i] = waitingRoom;
                    break;
                }
            }
            
            return waitingRoom;
        }
        
        public NetworkWaitingRoom GetRoomByID(int roomID){
            if(roomID < 0 || roomID >= MAX_ROOM) return null;
            return waitingRoomList[roomID];
        }
        
        public bool RemoveWaitingRoom(int id){
            if(id < 0 || id >= MAX_ROOM) return false;
            waitingRoomList[id] = null;
            return true;
        }
        
#endif
    }
}