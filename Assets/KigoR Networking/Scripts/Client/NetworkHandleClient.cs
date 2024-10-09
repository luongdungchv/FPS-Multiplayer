using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Kigor.Networking
{
    public partial class NetworkHandleClient
    {
        public static NetworkHandleClient Instance;
#if CLIENT_BUILD
        private Dictionary<PacketType, UnityAction<byte[]>> tcpHandleMap, udpHandleMap;

        public UnityAction<WaitingRoomStatePacket> OnWaitingRoomPacketReceived;
        public UnityAction<StartGamePacket> OnStartGamePacketReceived;
        public UnityAction<JoinWaitingRoomPacket> OnFailedToJoinWaitingRoom;
        public UnityAction<Scene, NetworkGameRoom> OnGameStart;
        public UnityAction<RoomStatePacket> OnRoomStateReceived;
        public UnityAction<RoomStatePacket> OnRoomStateInitialized;
        public UnityAction<int, PlayerState> OnReconcileStateReceived;
        public UnityAction<int> OnPlayerLeave;

        partial void Initialize();

        public NetworkHandleClient()
        {
            this.tcpHandleMap = new Dictionary<PacketType, UnityAction<byte[]>>
            {
                { PacketType.WAITING_ROOM_STATE, this.HandleWaitingRoomStatePacket },
                { PacketType.START_GAME, this.HandleStartGamePacket },
                { PacketType.UDP_CONNECTION_INFO, this.HandleServerUDPInfoPacket },
                { PacketType.ROOM_STATE, this.HandleInitialRoomStatePacket},
                { PacketType.PLAYER_LEAVE, this.HandlePlayerLeavePacket},
                
            };
            this.udpHandleMap = new Dictionary<PacketType, UnityAction<byte[]>>{
                {PacketType.LOCAL_PLAYER_STATE, this.HandleReconcilePacket},
                {PacketType.ROOM_STATE, this.HandleRoomStatePacket}
            };
            this.Initialize();
            Instance = this;
        }


        public void HandleTCPMessage(byte[] msg)
        {
            var packetType = (PacketType)msg[0];
            if (this.tcpHandleMap.ContainsKey(packetType))
            {
                this.tcpHandleMap[packetType].Invoke(msg);
            }
        }
        public void HandleUDPMessage(byte[] msg)
        {
            var packetType = (PacketType)msg[0];
            if (this.udpHandleMap.ContainsKey(packetType))
            {
                this.udpHandleMap[packetType].Invoke(msg);
            }
        }

        private void HandleWaitingRoomStatePacket(byte[] msg)
        {
            var packet = new WaitingRoomStatePacket();
            packet.DecodeMessage(msg);
            packet.playerNames.ForEach(x => Debug.Log(x));
            this.OnWaitingRoomPacketReceived?.Invoke(packet);
        }

        private void HandleStartGamePacket(byte[] msg)
        {
            var packet = new StartGamePacket();
            try
            {
                packet.DecodeMessage(msg);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            this.OnStartGamePacketReceived?.Invoke(packet);

            var scene = packet.mapName;
            Debug.Log(packet.rule.Value);
            var rule = NetworkGameRule.CreateRule(packet.rule);
            var room = new NetworkGameRoom();

            ThreadManager.ExecuteOnMainThread(() =>
            {
                Debug.Log("game start");
                SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive).completed += (op) =>
                {
                    try
                    {
                        var loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        packet.playerNameList.ForEach(x => Debug.Log(x));

                        room.SetScene(loadedScene);
                        room.SetRule(rule);

                        var tickScheduler = NetworkManager.Instance.CreateTickScheduler();
                        SceneManager.MoveGameObjectToScene(tickScheduler.gameObject, loadedScene);
                        rule.SetTickScheduler(tickScheduler);

                        for (int i = 0; i < packet.playerNameList.Count; i++)
                        {
                            var name = packet.playerNameList[i];
                            var player = NetworkManager.Instance.SpawnPlayer();
                            SceneManager.MoveGameObjectToScene(player.gameObject, SceneManager.GetSceneAt(SceneManager.sceneCount - 1));

                            player.SetName(name);
                            player.SetID(i);
                            player.SetRoom(room);

                            room.AddPlayer(player, i);
                        }

                        rule.Initialize(room.PlayerList, loadedScene);

                        this.OnGameStart?.Invoke(loadedScene, room);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }


                };
            });
        }



        private void HandleServerUDPInfoPacket(byte[] msg)
        {
            var packet = new UDPConnectionInfoPacket();
            try
            {
                packet.DecodeMessage(msg);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            NetworkTransport.Instance.InitializeUDP(packet.port);
        }
        private void HandleRoomStatePacket(byte[] msg)
        {
            var packet = new RoomStatePacket();
            try
            {
                packet.DecodeMessage(msg);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            this.OnRoomStateReceived?.Invoke(packet);
        }

        private void HandleInitialRoomStatePacket(byte[] msg)
        {
            var packet = new RoomStatePacket();
            Debug.Log("asdflfga");
            try
            {
                packet.DecodeMessage(msg);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            Debug.Log(packet.playerPositionList[0]);
            
            this.OnRoomStateInitialized += Test;
            this.OnRoomStateInitialized?.Invoke(packet);
            this.OnRoomStateInitialized -= Test;
        }
        private void Test(PacketData data){
            Debug.Log("ditmemay");
        }
        private void HandlePlayerLeavePacket(byte[] msg)
        {
            var packet = new PlayerLeavePacket();
            try
            {
                packet.DecodeMessage(msg);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            Debug.Log("Player leaving: " + packet.playerID);

            this.OnPlayerLeave?.Invoke(packet.playerID);
        }
        private void HandleReconcilePacket(byte[] msg){
            var packet = new LocalPlayerStatePacket();
            Debug.Log("Reconcile packet received");
            packet.DecodeMessage(msg);

            this.OnReconcileStateReceived?.Invoke(packet.tick, packet.playerState);
        }
#endif
    }
}
