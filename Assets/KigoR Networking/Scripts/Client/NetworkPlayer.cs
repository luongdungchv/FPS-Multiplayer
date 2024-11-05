using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class NetworkPlayer : MonoBehaviour
    {
        [SerializeField] private string playerName;

        [SerializeField] protected int id;
        public int PlayerID => this.id;

        public string Name => this.playerName;

        public void SetName(string name){
            this.playerName = name;
        }

        public void SetID(int id) => this.id = id;
        public PhysicsScene CurrentPhysicsScene => this.room.PhysicsScene;
#if CLIENT_BUILD
        private bool isLocalPlayer;
        public static NetworkPlayer localPlayer;
        public static int LocalPlayerID;
        protected NetworkGameRoom room;

        public bool IsLocalPlayer => this.isLocalPlayer;

        public void SetAsLocalPlayer(bool state){
            this.isLocalPlayer = state;
            if(state) localPlayer = this;
            this.LocalPlayerSetPostAction();
        }

        protected virtual void LocalPlayerSetPostAction(){

        }

        public void SetRoom(NetworkGameRoom room){
            this.room = room;
        }
        public void RemoveRoom(){
            this.room = null;
        }

        public void SendLeaveMessage(){
            var packet = new PlayerLeavePacket();
            packet.playerID = (byte)this.id;
            Debug.Log("Sending");
            NetworkTransport.Instance.SendPacketTCP(packet);
        }

#endif
    }
}
