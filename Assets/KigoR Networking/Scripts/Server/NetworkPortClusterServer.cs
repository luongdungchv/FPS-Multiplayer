using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.Events;

namespace Kigor.Networking
{
    public partial class NetworkPortCluster : MonoBehaviour
    {
        [SerializeField] private int port;
        [SerializeField] private int maxRoom;
        [SerializeField] private NetworkGameRoom[] rooms;

        public UnityAction TestEvent;



#if SERVER_BUILD


        private NetworkTransport networkTransport;
        private NetworkWaitingRoomManager waitingRoomManager;
        private NetworkGameRoomManager gameRoomManager;

        public NetworkTransport NetworkTransport => this.networkTransport;
        public NetworkWaitingRoomManager WaitingRoomManager => this.waitingRoomManager;
        public NetworkGameRoomManager GameRoomManager => this.gameRoomManager;

        public int Port => this.port;

        private void Awake()
        {
            this.networkTransport = new NetworkTransport(this);

            this.waitingRoomManager = new NetworkWaitingRoomManager(this);
            this.gameRoomManager = new NetworkGameRoomManager(this, this.maxRoom);

            this.rooms = new NetworkGameRoom[maxRoom];

            this.networkTransport.SetPort(this.port);
            this.networkTransport.StartServerTransport();

            this.StartCoroutine(this.IETest());
        }

        public NetworkGameRoom GetRoom(int index)
        {
            return rooms[index];
        }

        private void Update()
        {

        }

        IEnumerator IETest()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                this.TestEvent?.Invoke();
            }
        }



        public NetworkWaitingRoom CreateWaitingRoom()
        {
            var waitingRoom = new NetworkWaitingRoom();
            waitingRoom.SetOwnerCluster(this);
            return waitingRoom;
        }
#endif
    }
}
