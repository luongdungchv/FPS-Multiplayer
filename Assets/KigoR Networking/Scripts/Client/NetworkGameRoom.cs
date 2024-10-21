using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kigor.Networking
{
    public partial class NetworkGameRoom
    {
        private NetworkGameRule rule;
        private int roomID;
        private Scene scene;

        public int RoomID => this.roomID;
        public NetworkGameRule Rule => this.rule;   
        public PhysicsScene PhysicsScene => this.scene.GetPhysicsScene();

        public void SetRoomID(int id)
        {
            this.roomID = id;
        }
        public void SetRule(NetworkGameRule rule)
        {
            this.rule = rule;
        }

#if CLIENT_BUILD
        private Dictionary<int, NetworkPlayer> playersInRoom;
        public Dictionary<int, NetworkPlayer> PlayerList => this.playersInRoom;

        public NetworkGameRoom()
        {
            this.playersInRoom = new();
            NetworkHandleClient.Instance.OnPlayerLeave += this.RemovePlayer;
        }
        ~NetworkGameRoom()
        {
            Debug.Log("Room garbage collected");
        }

        public void SetScene(Scene scene) => this.scene = scene;

        public void AddPlayer(NetworkPlayer player, int id)
        {
            if (this.playersInRoom.ContainsKey(id)) return;
            this.playersInRoom.Add(id, player);
            Debug.Log(this.rule);
            this.rule.PlayerJoinCallback(player, id);

            NetworkTransport.Instance.OnServerCrash += this.HandleServerCrashCallback;
        }

        public void RemovePlayer(int id)
        {
            Debug.Log($"Player leaves: {id}");
            var playerToRemove = this.playersInRoom[id];
            playersInRoom.Remove(id);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                var isLocalPlayer = playerToRemove.IsLocalPlayer;
                UnityEngine.Object.Destroy(playerToRemove.gameObject);
                if (isLocalPlayer)
                {
                    this.DisposeRoom();

                    SceneManager.UnloadSceneAsync(this.scene).completed += (op) =>
                    {
                        GC.Collect();
                        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
                    };;
                }
            });
        }

        public void InitializeFirstPlayersBatch(List<NetworkPlayer> playerList, List<int> idList)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                this.playersInRoom.Add(idList[i], playerList[i]);
            }
        }

        public List<NetworkPlayer> GetPlayersList()
        {
            var result = new List<NetworkPlayer>();
            foreach (var id in this.playersInRoom.Keys)
            {
                result.Add(this.playersInRoom[id]);
            }
            return result;
        }

        private void HandleServerCrashCallback(SocketWrapper socket)
        {
            try
            {
                this.DisposeRoom();
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    SceneManager.UnloadSceneAsync(this.scene).completed += (op) =>
                    {
                        GC.Collect();
                        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
                    };
                });
                Debug.Log("crash");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void DisposeRoom()
        {
            NetworkTransport.Instance.OnServerCrash -= HandleServerCrashCallback;
            NetworkHandleClient.Instance.OnPlayerLeave -= this.RemovePlayer;
            foreach (var pair in this.playersInRoom)
            {
                pair.Value.RemoveRoom();
            }
            this.playersInRoom.Clear();
            this.playersInRoom = null;
            this.rule.Dispose();
            this.rule = null;
        }
#endif

    }
}
