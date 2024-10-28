using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

using System;
using UnityEngine.Events;

namespace Kigor.Networking
{
    public partial class NetworkPlayer : MonoBehaviour
    {
#if SERVER_BUILD
        protected SocketWrapper socket;
        protected NetworkGameRoom room;

        public SocketWrapper Socket => this.socket;
        public NetworkGameRoom Room => this.room;

        protected Dictionary<PacketType, UnityAction<byte[]>> msgHandler;
        ~NetworkPlayer(){
            Debug.Log("player resources released");
        }

        private bool disconnected;
        
        public virtual void Initialize(SocketWrapper socket, NetworkGameRoom room, int id){
            this.socket = socket;
            this.room = room;
            this.id = id;

            this.msgHandler = new Dictionary<PacketType,UnityAction<byte[]>>(){
                {PacketType.PLAYER_LEAVE, this.HandlePlayerLeave}
            };


            this.socket.RegisterUdpReceiveCallback(this.HandleMessage);
            this.socket.RegisterTcpReadCallback(this.HandleMessage);

            this.socket.RegisterTCPDisconnectCallback(this.HandleDisconnect);
            this.socket.RegisterUDPDisconnectCallback(this.HandleDisconnect);
        }

        protected void HandleMessage(SocketWrapper socket, byte[] message){
            var packetType = (PacketType)message[0];
            Debug.Log((packetType.Value, msgHandler.ContainsKey(packetType), message.Length));
            if(!msgHandler.ContainsKey(packetType)) return;
            msgHandler[packetType]?.Invoke(message);
        }

        public void UnbindRoom(){
            this.room = null;
        }

        private void HandleDisconnect(){
            Debug.Log("player disconnect");
            if(disconnected) return;
            try{
                // this.socket.DisconnectTCP();
                // this.socket.DisconnectUDP();
                this.room?.RemovePlayer(this.PlayerID);
                disconnected = true;
            }
            catch(Exception e){
                Debug.LogError(e);
            }

        }

        private void HandlePlayerLeave(byte[] msg){
            Debug.Log("Player left: " + this.id);
            Debug.Log(this.room);
            this.HandleDisconnect();
        }
#endif
    }
}
