using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using UnityEngine.Events;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Kigor.Networking
{
    public partial class NetworkTransport
    {
#if CLIENT_BUILD
        public static NetworkTransport Instance;

        public UnityAction<SocketWrapper> OnServerCrash;

        private SocketWrapper socket;

        public int SocketTCPPort => (socket.TCPSocket.Client.LocalEndPoint as IPEndPoint).Port;
        public int SocketUDPPort => (socket.UDPSocket.Client.LocalEndPoint as IPEndPoint).Port;

        public NetworkTransport()
        {
            Instance = this;
            this.Initialize();
        }

        public void Initialize()
        {
            this.socket = new SocketWrapper();
            this.socket.RegisterTcpReadCallback(this.HandleTCPReadCallback);
            this.socket.RegisterTCPDisconnectCallback(this.HandleServerCrash);
            this.socket.RegisterUdpReceiveCallback(this.HandleUDPReceiveCallback);
        }

        public void ConnectTcpToServer(string host, int port, UnityAction successCallback)
        {
            this.socket.ConnectTcpToServer(host, port, successCallback);
        }
        public void ConnectTcpToServer(IPAddress ip, int port, UnityAction successCallback)
        {
            this.socket.ConnectTcpToServer(ip, port, successCallback);
        }
        public void InitializeUDP(IPAddress ip, int port)
        {
            this.socket.AddUDPRemoteEndpoint(ip, port);
        }
        public void InitializeUDP(int port)
        {
            var ip = (this.socket.TCPSocket.Client.RemoteEndPoint as IPEndPoint).Address;
            this.socket.AddUDPRemoteEndpoint(ip, port);
        }
        public void Disconnect(UnityAction callback = null)
        {
            try
            {
                this.socket.DisconnectTCP(callback);
                Debug.Log("disconenct");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void SendPacketTCP(PacketData packet, UnityAction successCallback = null)
        {
            var data = packet.EncodeData();
            this.socket.SendDataTCP(data, successCallback);
        }
        public void SendPacketUDP(PacketData packet, UnityAction successCallback = null)
        {
            var data = packet.EncodeData();
            this.socket.SendDataUDP(data, successCallback);
        }

        private void HandleTCPReadCallback(SocketWrapper socket, byte[] data)
        {
            NetworkHandleClient.Instance.HandleTCPMessage(data);
        }
        private void HandleUDPReceiveCallback(SocketWrapper socket, byte[] data)
        {
            NetworkHandleClient.Instance.HandleUDPMessage(data);
        }
        private void HandleServerCrash()
        {
            Debug.Log("server down");
            this.socket.DisposeSocket();
            this.Initialize();
            this.OnServerCrash?.Invoke(this.socket);
            GC.Collect();
        }

#endif
    }
}
