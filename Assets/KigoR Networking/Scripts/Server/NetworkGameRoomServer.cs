using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System;

namespace Kigor.Networking
{
    public partial class NetworkGameRoom
    {


#if SERVER_BUILD
        private NetworkPlayer[] playerList;
        private NetworkPortCluster cluster;
        private int maxPlayer = 10;
        public NetworkTransport NetworkTransport => cluster.NetworkTransport;
        public int Port => this.cluster.Port;

        public NetworkGameRoom(NetworkPortCluster cluster, Scene scene)
        {
            this.cluster = cluster;
            this.scene = scene;
            this.playerList = new NetworkPlayer[this.maxPlayer];
        }
        ~NetworkGameRoom()
        {
            Debug.Log($"Room resource released, ID: {this.roomID}");
        }


        public void AddPlayer(SocketWrapper socket)
        {
            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i] == null)
                {
                    var player = NetworkManager.Instance.SpawnPlayer();
                    player.Initialize(socket, this, i);
                    this.playerList[i] = player;
                    player.SetID(i);
                    this.rule.PlayerJoinCallback(player, i);
                    return;
                }
            }
        }
        public void RemovePlayer(int id)
        {
            var removedPlayer = this.playerList[id];
            if (removedPlayer == null) return;
            removedPlayer.UnbindRoom();
            this.playerList[id] = null;
            this.BroadcastPlayerLeaveMessage(id);
            this.rule.PlayerLeaveCallback(id);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(removedPlayer.gameObject);
                foreach (var player in this.playerList)
                {
                    if (player != null)
                    {
                        return;
                    }
                }
                this.DisposeRoom();
                NetworkManager.Instance.RemoveScene(this.scene);
                SceneManager.UnloadSceneAsync(this.scene).completed += (op) => {
                    GC.Collect();
                    SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
                };
            });
        }

        public void InitializeGameplay(List<SocketWrapper> socketList, NetworkGameRule rule, Scene loadedScene)
        {
            playerList = new NetworkPlayer[this.maxPlayer];
            for (int i = 0; i < socketList.Count; i++)
            {
                playerList[i] = NetworkManager.Instance.SpawnPlayer();
                SceneManager.MoveGameObjectToScene(playerList[i].gameObject, this.scene);
                playerList[i].Initialize(socketList[i], this, i);
            }

            this.SetRule(rule);

            var dict = new Dictionary<int, NetworkPlayer>();
            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i] == null) continue;
                dict.Add(i, playerList[i]);
            }

            rule.Initialize(dict, loadedScene);
        }

        public void BroadcastPlayerLeaveMessage(int playerID)
        {
            var packet = new PlayerLeavePacket();
            packet.playerID = (byte)playerID;
            var data = packet.EncodeData();
            foreach (var player in this.playerList)
            {
                if (player == null) continue;
                player.Socket.SendDataTCP(data);
            }
            Debug.Log("Leave msg broadcast done");
        }

        public void DisposeRoom()
        {
            Debug.Log($"Disposing room: {this.roomID}");
            this.cluster.GameRoomManager.RemoveRoom(this.roomID);
            this.rule.Dispose();
            this.rule = null;
            for (int i = 0; i < this.playerList.Length; i++)
            {
                var player = playerList[i];
                if (player == null) continue;
                player.UnbindRoom();
                playerList[i] = null;
            }


        }


        public void SetOwnerCluster(NetworkPortCluster cluster)
        {
            this.cluster = cluster;
        }
#endif
    }
}
