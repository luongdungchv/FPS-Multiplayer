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
        partial void Initialize(){
            this.udpHandleMap.Add(PacketType.FPS_RECONCILE_PACKET, HandleFPSReconciliation);
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
#endif
    }
}
