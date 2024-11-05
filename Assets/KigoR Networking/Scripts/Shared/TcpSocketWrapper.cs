using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Net;
using System.Text;

namespace Kigor.Networking
{
    public class SocketWrapper
    {
        private TcpClient tcpSocket;
        private UdpClient udpSocket;
        private NetworkStream tcpStream;
        private UnityAction<SocketWrapper, byte[]> OnTCPDataRead;
        private UnityAction<SocketWrapper, byte[]> OnUDPDataReceived;
        private UnityAction OnTCPDisconnect;
        private UnityAction OnUDPDisconnect;

        private UnityAction<SocketWrapper> OnClientDisconnect;
        private byte[] receiveBuffer;

        private const int BUFFER_SIZE = 1024;

        public TcpClient TCPSocket => this.tcpSocket;

        public UdpClient UDPSocket => this.udpSocket;


        #region Constructors
        public SocketWrapper(TcpClient tcpSocket)
        {
            this.tcpSocket = tcpSocket;
            tcpStream = tcpSocket.GetStream();
            this.receiveBuffer = new byte[BUFFER_SIZE];
            this.udpSocket = new UdpClient(0);
        }

        public SocketWrapper(UdpClient udpSocket)
        {
            this.udpSocket = udpSocket;
            this.tcpSocket = new TcpClient();
        }

        public SocketWrapper(TcpClient tcpSocket, UdpClient udpSocket) : this(tcpSocket)
        {
            this.udpSocket = udpSocket;
            this.tcpSocket = tcpSocket;
        }

        public SocketWrapper()
        {
            this.tcpSocket = new TcpClient();
            this.udpSocket = new UdpClient(0);
            this.receiveBuffer = new byte[BUFFER_SIZE];
        }
        #endregion

        #region Connections
        public async void ConnectTcpToServer(string host, int port, UnityAction successCallback = null)
        {
            this.tcpSocket = new TcpClient();
            Debug.Log($"Connecting to {host}:{port}");
            try
            {
                await this.tcpSocket.ConnectAsync(host, port);
                successCallback?.Invoke();
                this.tcpStream = this.tcpSocket.GetStream();
                this.StartReadingTCP();
            }
            catch (Exception e)
            {
                Debug.Log("Failed to connect to server!");
                Debug.LogError(e);
            }
        }


        public async void ConnectTcpToServer(IPAddress ip, int port, UnityAction successCallback = null)
        {
            this.tcpSocket = new TcpClient();
            Debug.Log($"Connecting to {ip}:{port}");
            try
            {
                await this.tcpSocket.ConnectAsync(ip, port);
                this.tcpStream = this.tcpSocket.GetStream();
                this.StartReadingTCP();
                successCallback?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log("Failed to connect to server!");
                Debug.LogError(e);
            }
        }

        public void AddUDPRemoteEndpoint(IPAddress ip, int port){
            this.udpSocket.Connect(ip, port);
            this.StartReceivingUDP();
        }

        public void DisconnectTCP()
        {
            this.tcpSocket.Client.Shutdown(SocketShutdown.Both);
            this.tcpSocket.Client.Disconnect(false);
            OnTCPDisconnect?.Invoke();
        }

        public void DisconnectTCP(UnityAction callback = null)
        {
            this.tcpSocket.Client.Shutdown(SocketShutdown.Both);
            this.tcpSocket.Client.Disconnect(false);
            callback?.Invoke();
        }

        public void DisconnectUDP(){
            this.udpSocket.Close();
            this.OnUDPDisconnect?.Invoke();
        }

        public void DisposeSocket()
        {
            this.tcpSocket?.Close();
            this.tcpSocket?.Dispose();

            this.udpSocket?.Close();
            this.udpSocket?.Dispose();
        }

        #endregion

        #region Event_Registration
        public void RegisterTcpReadCallback(UnityAction<SocketWrapper, byte[]> callback)
        {
            this.OnTCPDataRead += callback;
        }
        public void UnregisterTcpReadCallback(UnityAction<SocketWrapper, byte[]> callback)
        {
            this.OnTCPDataRead -= callback;
        }
        public void RegisterUdpReceiveCallback(UnityAction<SocketWrapper, byte[]> callback)
        {
            this.OnUDPDataReceived += callback;
        }
        public void UnregisterUdpReceiveCallback(UnityAction<SocketWrapper, byte[]> callback)
        {
            this.OnUDPDataReceived -= callback;
        }

        public void RegisterTCPDisconnectCallback(UnityAction callback)
        {
            this.OnTCPDisconnect += callback;
        }
        public void UnregisterTCPDisconnectCallback(UnityAction callback)
        {
            this.OnTCPDisconnect -= callback;
        }
        public void RegisterUDPDisconnectCallback(UnityAction callback)
        {
            this.OnUDPDisconnect += callback;
        }
        public void UnregisterUDPDisconnectCallback(UnityAction callback)
        {
            this.OnUDPDisconnect -= callback;
        }
        #endregion

        #region Data_Communication

        public void StartReadingTCP()
        {
            tcpStream.BeginRead(receiveBuffer, 0, BUFFER_SIZE, new AsyncCallback(TcpReadCallback), null);
        }
        public void StartReceivingUDP()
        {
            Debug.Log("start");
            this.udpSocket.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
        }

        public void SendDataTCP(byte[] data, UnityAction completeCallback = null)
        {
            tcpStream.BeginWrite(data, 0, data.Length, new AsyncCallback(result =>
            {
                this.tcpStream.EndWrite(result);
                Debug.Log($"Stream Written Successful {data.Length}");
                completeCallback?.Invoke();
            }), null);
        }
        public void SendDataUDP(byte[] data, UnityAction callback = null)
        {
            this.udpSocket.BeginSend(data, data.Length, (result) =>
            {
                var sent = this.udpSocket.EndSend(result);
                Debug.Log($"UDP data sent successfully from {this.udpSocket.Client.LocalEndPoint as IPEndPoint} to: {this.udpSocket.Client.RemoteEndPoint as IPEndPoint}, {data.Length} bytes");
                callback?.Invoke();
            }, null);
        }
        private void TcpReadCallback(IAsyncResult result)
        {
            var dataLength = tcpStream.EndRead(result);
            Debug.Log(dataLength);
            if (dataLength == 0)
            {
                //TODO: Handle disconnect
                this.DisconnectTCP();
                return;
            }

            Debug.Log($"Received {dataLength} bytes from {this.tcpSocket.Client.RemoteEndPoint}");

            try
            {
                var readIndex = 0;
                var readBytes = new List<byte>();
                while (readIndex < dataLength)
                {
                    var msgLength = this.receiveBuffer[readIndex];
                    for (int i = readIndex + 1; i < readIndex + msgLength + 1; i++)
                    {
                        readBytes.Add(this.receiveBuffer[i]);
                    }

                    this.OnTCPDataRead?.Invoke(this, readBytes.ToArray());
                    readIndex += msgLength + 1;
                    readBytes.Clear();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            tcpStream.BeginRead(receiveBuffer, 0, BUFFER_SIZE, new AsyncCallback(TcpReadCallback), null);
        }

        private void UdpReceiveCallback(IAsyncResult result)
        {
            try
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, 0);
                var data = this.udpSocket.EndReceive(result, ref remoteEP);

                if (data.Length == 0)
                {
                    this.udpSocket.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
                    return;
                }

                Debug.Log($"UDP data received: {data.Length} bytes");
                this.OnUDPDataReceived?.Invoke(this, data);
                this.udpSocket.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.Log((this.udpSocket.Client.LocalEndPoint, this.udpSocket.Client.RemoteEndPoint));
                this.DisconnectUDP();
                //this.OnUDPDisconnect?.Invoke();
            }
        }

        private void TcpWriteCallback(IAsyncResult result)
        {
            this.tcpStream.EndWrite(result);
        }
        #endregion
    }
}
