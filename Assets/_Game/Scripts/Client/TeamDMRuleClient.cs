﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kigor.Networking
{
    public partial class TeamDMRule
    {
        #if CLIENT_BUILD
        private RoomStatePacket pendingProcessState;

        public TeamDMRule()
        {
            NetworkHandleClient.Instance.OnRoomStateInitialized += RoomStateInitCallback;
            NetworkHandleClient.Instance.OnRoomStateReceived += RoomStateReceivedCallback;
            this.players = new();
        }
        public override void PlayerJoinCallback(NetworkPlayer newPlayer, int id)
        {
            this.players.Add(id, newPlayer);
        }

        public override void Initialize(Dictionary<int, NetworkPlayer> initialPlayers, Scene loadedScene)
        {
            this.players = initialPlayers;
            
            if (pendingProcessState == null) return;
            try
            {
                for (int i = 0; i < pendingProcessState.playerIDList.Count; i++)
                {
                    var player = this.players[pendingProcessState.playerIDList[i]];
                    player.SetID(pendingProcessState.playerIDList[i]);
                    var playerFPS = (player as NetworkFPSPlayer);
                    // player.transform.position = pendingProcessState.playerPositionList[i];
                    // player.transform.eulerAngles = pendingProcessState.playerRotationList[i];
                    playerFPS.SetNonLocalState(new FPSPlayerState()
                    {
                        position = pendingProcessState.playerPositionList[i],
                        horizontalRotation = this.pendingProcessState.playerRotationList[i].y,
                        verticalRotation = this.pendingProcessState.playerRotationList[i].x
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public override void PlayerLeaveCallback(int id)
        {
            
        }

        public override void Dispose()
        {
            this.players = null;
            this.pendingProcessState = null;

            NetworkHandleClient.Instance.OnRoomStateInitialized -= RoomStateInitCallback;
            NetworkHandleClient.Instance.OnRoomStateReceived -= RoomStateReceivedCallback;
        }

        private void RoomStateInitCallback(RoomStatePacket packet)
        {
            Debug.Log("Room state init callback");
            this.pendingProcessState = packet;

            if (players == null || players.Count == 0) return;
        }

        private void RoomStateReceivedCallback(RoomStatePacket packet){
            Debug.Log("RoomStateReceivedCallback");
            try
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    for (int i = 0; i < packet.playerIDList.Count; i++)
                    {
                        var player = this.players[packet.playerIDList[i]];
                        if(player.IsLocalPlayer) continue;
                        player.SetID(packet.playerIDList[i]);
                        player.transform.position = packet.playerPositionList[i];
                        player.transform.eulerAngles = packet.playerRotationList[i];
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        #endif
    }
}