using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using UnityEngine.Events;


namespace Kigor.Networking
{
    public partial class NetworkWaitingPlayer
    {
#if SERVER_BUILD
        private SocketWrapper socket;
        private NetworkWaitingRoom room;
        private MessageHandler msgHandler;

        private bool ready;
        private bool isRoomMaster;

        public SocketWrapper Socket => this.socket;
        public bool Ready => this.ready;
        public bool IsRoomMaster => this.isRoomMaster;

        public NetworkWaitingPlayer(SocketWrapper socket, NetworkWaitingRoom room, string name)
        {
            this.socket = socket;
            this.room = room;
            this.playerName = name;

            this.msgHandler = new MessageHandler(this);

            this.socket.RegisterTcpReadCallback(this.HandleTCPMessage);
            this.socket.StartReadingTCP();
            this.socket.RegisterTCPDisconnectCallback(this.HandlePlayerDisconnect);
        }

        public void Dispose()
        {
            this.socket.UnregisterTcpReadCallback(this.HandleTCPMessage);
            this.socket.UnregisterTCPDisconnectCallback(this.HandlePlayerDisconnect);
        }
        public void SetMaster(bool state)
        {
            this.isRoomMaster = state;
            this.ready = state;
        }

        private void HandleTCPMessage(SocketWrapper socket, byte[] data)
        {
            this.msgHandler.HandleMessage(data);
        }

        private void HandlePlayerDisconnect()
        {
            this.room.RemoveWaitingPlayer(this);
        }

        public void LeaveCurrentRoom()
        {
            this.room.RemoveWaitingPlayer(this);
        }
        public void TriggerStartGame()
        {
            this.room.StartGame();
        }
        public void SetRoomRule(GameRule rule)
        {
            this.room.SetRule(rule);
        }
        public void SetReady(bool state, bool broadcastState = true)
        {
            this.ready = state;
            if (broadcastState) this.room.BroadcastRoomState();
        }

        public class MessageHandler
        {
            private Dictionary<PacketType, UnityAction<byte[]>> handlerMap;
            private NetworkWaitingPlayer owner;

            public MessageHandler(NetworkWaitingPlayer owner)
            {
                this.handlerMap = new Dictionary<PacketType, UnityAction<byte[]>>();
                this.owner = owner;

                handlerMap.Add(PacketType.LEAVE_WAITING_ROOM, this.HandleLeaveWaitingRoom);
                handlerMap.Add(PacketType.READY, this.HandleReady);
                handlerMap.Add(PacketType.START_GAME, this.HandleStartGame);
            }

            public void HandleMessage(byte[] msg)
            {
                var type = (PacketType)msg[0];
                Debug.Log($"Incoming message {type.Value}");
                try
                {
                    if (!handlerMap.ContainsKey(type)) return;
                    handlerMap[type].Invoke(msg);
                }
                catch (System.Exception e){
                    Debug.Log(e);
                }
            }

            private void HandleLeaveWaitingRoom(byte[] msg)
            {
                var packet = new LeaveWaitingRoomPacket();
                packet.DecodeMessage(msg);
                this.owner.LeaveCurrentRoom();
            }
            private void HandleReady(byte[] msg)
            {
                if (owner.IsRoomMaster) return;
                var packet = new ReadyPacket();
                Debug.Log($"Ready packet: {packet.ready}");
                try
                {
                    packet.DecodeMessage(msg);
                    this.owner.SetReady(packet.ready);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
            }
            private void HandleStartGame(byte[] msg)
            {
                if (owner.IsRoomMaster)
                {
                    var packet = new StartGamePacket();
                    packet.DecodeMessage(msg);
                    owner.SetRoomRule(packet.rule);
                    owner.TriggerStartGame();
                }
            }
        }
#endif
    }
}
