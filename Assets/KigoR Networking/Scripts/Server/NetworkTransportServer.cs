using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine.Events;
using System.Security;

namespace Kigor.Networking
{
    public partial class NetworkTransport
    {
#if SERVER_BUILD
        private TcpListener tcpListener;
        private UdpClient udpListener;
        private NetworkPortCluster cluster;

        private UnityAction<TcpClient> OnClientConnected;
        private Dictionary<TcpClient, SocketWrapper> connectedClientList;

        private int port;

        public NetworkTransport(NetworkPortCluster cluster)
        {
            this.cluster = cluster;
        }
        #region PUBLIC_METHOD 
        public void SetPort(int port)
        {
            this.port = port;
        }

        public void StartServerTransport()
        {
            Debug.Log($"Starting server on port {this.port}");
            tcpListener = new TcpListener(IPAddress.Any, this.port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(this.TcpAcceptClientCallback, null);
            Debug.Log($"Server started on port {this.port}");
        }
        public void RegisterClientConnectCallback(UnityAction<TcpClient> callback)
        {
            if (this.OnClientConnected == null) OnClientConnected = (client) => { callback.Invoke(client); };
            else this.OnClientConnected += callback;
        }

        public void RemoveClient(TcpClient socket)
        {
            if (!this.connectedClientList.ContainsKey(socket)) return;
            this.connectedClientList.Remove(socket);
        }


        public void SendMessageUDP(UdpClient udp, IPEndPoint remoteEP, byte[] msg)
        {
            udp.BeginSend(msg, msg.Length, remoteEP, null, null);
        }
        #endregion

        private void TcpAcceptClientCallback(IAsyncResult result)
        {
            var client = tcpListener.EndAcceptTcpClient(result);
            Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}");
            var socketWrapper = new SocketWrapper(client);
            socketWrapper.RegisterTcpReadCallback(this.HandleInitialMessage);
            socketWrapper.StartReadingTCP();
            tcpListener.BeginAcceptTcpClient(this.TcpAcceptClientCallback, null);
        }

        private void HandleInitialMessage(SocketWrapper socket, byte[] msg)
        {
            var msgType = (PacketType)msg[0];
            Debug.Log($"Incoming message {msgType}");

            socket.UnregisterTcpReadCallback(this.HandleInitialMessage);

            if (msgType == PacketType.JOIN_WAITING_ROOM)
            {
                try
                {
                    var packet = new JoinWaitingRoomPacket();
                    packet.DecodeMessage(msg);
                    Debug.Log($"UDP Port: {packet.udpPort}");
                    var room = this.cluster.WaitingRoomManager.GetRoomByID(packet.roomID);
                    if (room == null) return;
                    var player = room.AddWaitingPlayer(socket, packet);
                    if (player == null)
                    {
                        packet.name = "0";
                        packet.state = false;
                        packet.roomID = (short)room.RoomID;
                        var data = packet.EncodeData();
                        socket.SendDataTCP(data);
                    }
                    else
                    {
                        socket.AddUDPRemoteEndpoint((socket.TCPSocket.Client.RemoteEndPoint as IPEndPoint).Address, (int)packet.udpPort);
                        var udpInfoPacket = new UDPConnectionInfoPacket();
                        udpInfoPacket.port = (ushort)((socket.UDPSocket.Client.LocalEndPoint as IPEndPoint).Port);
                        var data = udpInfoPacket.EncodeData();
                        socket.SendDataTCP(data);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else if (msgType == PacketType.CREATE_WAITING_ROOM)
            {
                var packet = new CreateWaitingRoomPacket();
                packet.DecodeMessage(msg);
                Debug.Log($"UDP Port: {packet.udpPort}");
                socket.AddUDPRemoteEndpoint((socket.TCPSocket.Client.RemoteEndPoint as IPEndPoint).Address, (int)packet.udpPort);
                var newRoom = this.cluster.WaitingRoomManager.AddWaitingRoom();
                newRoom.SetMap("TestDM");
                var newPlayer = newRoom.AddWaitingPlayer(socket, packet.playerName, true);

                var udpInfoPacket = new UDPConnectionInfoPacket();
                udpInfoPacket.port = (ushort)((socket.UDPSocket.Client.LocalEndPoint as IPEndPoint).Port);
                var data = udpInfoPacket.EncodeData();
                socket.SendDataTCP(data);
            }


        }
#endif
    }

}