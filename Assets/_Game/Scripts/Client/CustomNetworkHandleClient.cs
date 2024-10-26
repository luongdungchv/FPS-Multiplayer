using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kigor.Networking
{
    public partial class NetworkHandleClient
    {
#if CLIENT_BUILD
        public UnityAction<int, FPSPlayerState> OnReconcilePacketReceived;
        public UnityAction<byte, Vector3> OnPlayerShotPacketReceived;
        partial void Initialize(){
            this.udpHandleMap.Add(PacketType.FPS_RECONCILE_PACKET, HandleFPSReconciliation);
            this.tcpHandleMap.Add(PacketType.FPS_PLAYER_SHOT, this.HandlePlayerShot);
        }

        private void HandleFPSReconciliation(byte[] msg){
            var packet = new FPSReconcilePacket();
            try{
                packet.DecodeMessage(msg);
            }
            catch(System.Exception e){
                Debug.LogError(e);
            }
            
            this.OnReconcilePacketReceived?.Invoke(packet.tick, packet.playerState);
        }

        private void HandlePlayerShot(byte[] msg)
        {
            
            var packet = new FPSPlayerShotPacket();
            try{
                packet.DecodeMessage(msg);
            }
            catch(System.Exception e){
                Debug.LogError(e);
            }
            Debug.Log($"Player shot received: {packet.playerID}, {packet.hitPos}");

            this.OnPlayerShotPacketReceived?.Invoke(packet.playerID, packet.hitPos);
        }
#endif
    }
}
