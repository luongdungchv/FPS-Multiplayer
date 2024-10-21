using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kigor.Networking
{
    public partial class FreeForAllRule : NetworkGameRule
    {
        public void Test()
        {

        }
#if SERVER_BUILD
        ~FreeForAllRule(){
            Debug.Log("Rule resource released: free for all");
        }
        //private TickScheduler tickScheduler;
        //public override TickScheduler TickScheduler => this.tickScheduler;
        public override void PlayerJoinCallback(NetworkPlayer newPlayer, int id)
        {
            this.players.Add(id, newPlayer);
        }
        public override void PlayerLeaveCallback(int id)
        {
            this.players.Remove(id);
        }
        public override void Dispose()
        {
            this.players = null;
            tickScheduler.UnregisterTickCallback(this.TickUpdate);
            this.tickScheduler = null;
        }

        public override void Initialize(Dictionary<int, NetworkPlayer> intialPlayers, Scene loadedScene)
        {
            this.players = intialPlayers;

            this.tickScheduler = NetworkManager.Instance.CreateTickScheduler();
            SceneManager.MoveGameObjectToScene(tickScheduler.gameObject, loadedScene);
            tickScheduler.RegisterTickCallback(this.TickUpdate);

            var packet = new RoomStatePacket();
            foreach (var id in this.players.Keys)
            {
                var player = players[id];
                if (player == null) continue;

                var x = Random.Range(0f, 20f);
                var z = Random.Range(0f, 20f);
                var pos = new Vector3(-11, 1, 23);
                player.transform.position = pos;

                packet.playerPositionList.Add(player.transform.position);
                packet.playerRotationList.Add(player.transform.eulerAngles);
                packet.playerIDList.Add(id);
            }
            var data = packet.EncodeDataTCP();
            foreach (var pair in this.players)
            {
                var player = pair.Value;
                if (player == null) continue;
                player.Socket.SendDataTCP(data);
                Debug.Log("Room Init Sent");
            }
        }

        public void TickUpdate()
        {
            this.BroadcastRoomStatePacket();
        }
        private void BroadcastRoomStatePacket()
        {
            var packet = new RoomStatePacket();
            foreach (var i in this.players.Keys)
            {
                var player = players[i];
                if (player == null) continue;
                packet.playerPositionList.Add(player.transform.position);
                packet.playerRotationList.Add(player.transform.eulerAngles);
                packet.playerIDList.Add(i);
            }
            var data = packet.EncodeData();
            foreach (var pair in this.players)
            {
                var player = pair.Value;
                if (player == null) continue;
                player.Socket.SendDataUDP(data);
            }
        }
#endif

    }
}
