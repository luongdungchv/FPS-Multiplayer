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
        public UnityAction<int, int> OnWeaponChange;
        public UnityAction<int> OnPlayerDie;
        public UnityAction<int, Vector3> OnShotMessageReceived;
        
        partial void Initialize(){
            this.udpHandleMap.Add(PacketType.FPS_RECONCILE_PACKET, HandleFPSReconciliation);
            
            this.tcpHandleMap.Add(PacketType.FPS_PLAYER_SHOT, this.HandlePlayerShot);
            this.tcpHandleMap.Add(PacketType.FPS_WEAPON_CHANGE, this.HandleWeaponChangePacket);
            this.tcpHandleMap.Add(PacketType.FPS_PLAYER_DIE, this.HandlePlayerDiePacket);
            this.tcpHandleMap.Add(PacketType.FPS_SERVER_RESPOND_SHOT, this.HandleShotPacket);
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
        private void HandleWeaponChangePacket(byte[] msg)
        {
            Debug.Log("Weapon change packet received");
            var packet = new FPSWeaponChangePacket();
            try
            {
                packet.DecodeMessage(msg);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            
            this.OnWeaponChange?.Invoke(packet.playerID, (int)packet.weapon);
        }

        private void HandlePlayerDiePacket(byte[] msg)
        {
            var packet = new FPSPlayerDiePacket();
            try
            {
                packet.DecodeMessage(msg);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            this.OnPlayerDie?.Invoke(packet.playerID);
        }

        private void HandleShotPacket(byte[] msg)
        {
            var packet = new FPSServerRespondShotPacket();
            try
            {
                packet.DecodeMessage(msg);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            this.OnShotMessageReceived?.Invoke(packet.playerID, packet.endPos);
        }
        
#endif
    }
}
