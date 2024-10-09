using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kigor.Networking
{
    public class NetworkGameRoomManager
    {
#if SERVER_BUILD
        private NetworkPortCluster owner;
        
        private NetworkGameRoom[] roomList;
        
        
        public NetworkGameRoomManager(NetworkPortCluster owner, int roomCount){
            this.owner = owner;
            this.roomList = new NetworkGameRoom[roomCount];
        }
        
        public NetworkGameRoom AddRoom(Scene scene, NetworkGameRule rule){
            for(int i = 0; i < roomList.Length; i++){
                if(roomList[i] == null){
                    var room = new NetworkGameRoom(this.owner, scene);

                    room.SetRoomID(i);
                    room.SetRule(rule);

                    var tickScheduler = NetworkManager.Instance.CreateTickScheduler();
                    SceneManager.MoveGameObjectToScene(tickScheduler.gameObject, scene);

                    roomList[i] = room;
                    return room;
                }
            }
            return null;
        }

        public void RemoveRoom(int roomID){
            roomList[roomID] = null;
        }
#endif
    }
}
