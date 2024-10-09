using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using System.Text;

namespace Kigor.Networking
{
    public partial class NetworkWaitingRoom
    {
#if SERVER_BUILD
        private GameRule rule;
        private NetworkPortCluster cluster;
        


        private string gameMap = "Server_TestMap";

        private int maxPlayer = 10;

        public NetworkWaitingRoom()
        {
            this.waitingPlayerList = new List<NetworkWaitingPlayer>();
            //this.rule = new FreeForAllRule();
            
        }

        public void SetMap(string map)
        {
            this.gameMap = map;
        }
        public void SetOwnerCluster(NetworkPortCluster cluster){
            this.cluster = cluster;
            cluster.TestEvent += this.Test;
        }
        public void SetRule(GameRule ruleType)
        {
            this.rule = ruleType;
        }

        private void Test(){
            
        }

        public void StartGame()
        {
            foreach (var player in this.waitingPlayerList)
            {
                if (!player.Ready) return;
            }

            this.cluster.WaitingRoomManager.RemoveWaitingRoom(this.roomID);

            Debug.Log($"Game starting: {gameMap}");

            ThreadManager.ExecuteOnMainThread(() =>
            {
                var loadParams = new LoadSceneParameters(){
                    loadSceneMode = LoadSceneMode.Additive,
                    localPhysicsMode = LocalPhysicsMode.Physics3D
                };
                SceneManager.LoadSceneAsync(this.gameMap, loadParams).completed += (op) =>
                {
                    var loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                    NetworkManager.Instance.AddScene(loadedScene);
                    var rule = NetworkGameRule.CreateRule(this.rule);
                    var gameRoom = this.cluster.GameRoomManager.AddRoom(loadedScene, rule);

                    var playerList = this.waitingPlayerList.Select(x =>
                    {
                        x.Dispose();
                        Debug.Log(x.Socket.UDPSocket.Client.RemoteEndPoint);
                        return x.Socket;
                    }).ToList();

                    Debug.Log(this.rule.Value);
                    this.BroadcastStartGameMsg(); 
                    gameRoom.InitializeGameplay(playerList, rule, loadedScene);
                    //gameRoom.DisposeRoom();
                };
            });
            
        }

        public NetworkWaitingPlayer AddWaitingPlayer(SocketWrapper socket, JoinWaitingRoomPacket packet)
        {
            return this.AddWaitingPlayer(socket, packet.name);
        }
        public NetworkWaitingPlayer AddWaitingPlayer(SocketWrapper socket, string playerName, bool isMaster = false)
        {
            if(this.waitingPlayerList.Count == this.maxPlayer) return null;
            foreach(var i in this.waitingPlayerList){
                if(i.PlayerName == playerName) return null;
            }
            var player = new NetworkWaitingPlayer(socket, this, playerName);
            player.SetMaster(isMaster);
            this.waitingPlayerList.Add(player);
            this.BroadcastRoomState();
            return player;
        }

        public void RemoveWaitingPlayer(NetworkWaitingPlayer player)
        {
            this.waitingPlayerList.Remove(player);
            Debug.Log($"Player removed {player.PlayerName}");
            if (this.waitingPlayerList.Count == 0)
            {
                this.cluster.WaitingRoomManager.RemoveWaitingRoom(this.roomID);
            }
            this.BroadcastRoomState();
        }

        public void BroadcastRoomState()
        {
            var playerNameList = this.waitingPlayerList.Select((player) => player.PlayerName).ToList();
            var playerReadyStateList = this.waitingPlayerList.Select((player) => player.Ready).ToList();
            waitingPlayerList.ForEach(x => Debug.Log(x.PlayerName));
            var packet = new WaitingRoomStatePacket(playerNameList, (short)this.roomID);
            packet.readyStates = playerReadyStateList;
            
            try
            {
                var data = packet.EncodeData();
                foreach (var player in this.waitingPlayerList)
                {
                    var socket = player.Socket;
                    socket.SendDataTCP(data);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

        }

        public void BroadcastStartGameMsg()
        {
            var packet = new StartGamePacket();
            var playerNameList = this.waitingPlayerList.Select((player) => player.PlayerName).ToList();
            packet.mapName = this.gameMap;
            packet.playerNameList = playerNameList;
            packet.rule = this.rule;
            try
            {
                var data = packet.EncodeData();
                foreach (var player in this.waitingPlayerList)
                {
                    player.Socket.SendDataTCP(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

#endif
    }
}
