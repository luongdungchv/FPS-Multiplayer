using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kigor.Networking;

using System.Net;
using Unity.VisualScripting;

public class UIJoinRoom : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField,portInputField, roomInputField, playerName;
    [SerializeField] private Button joinBtn, createBtn;
#if CLIENT_BUILD
    
    public void Init(){
        NetworkHandleClient.Instance.OnFailedToJoinWaitingRoom += this.HandleJoinWaitingRoomFailed;
        this.joinBtn.onClick.AddListener(this.HandleJoinBtnClick);
        this.createBtn.onClick.AddListener(this.HandleCreateBtnClick);
    }
    
    private void HandleJoinBtnClick(){
        Debug.Log("asdf");
        NetworkTransport.Instance.ConnectTcpToServer(IPAddress.Parse(this.ipInputField.text), int.Parse(this.portInputField.text), () => {
            var packet = new JoinWaitingRoomPacket();
            packet.name = playerName.text;
            packet.udpPort = (ushort)NetworkTransport.Instance.SocketUDPPort;
            Debug.Log(packet.udpPort);
            packet.roomID = short.Parse(roomInputField.text);
            NetworkTransport.Instance.SendPacketTCP(packet);
            NetworkClientInfoHolder.Instance.playerName = this.playerName.text;
        });
    }
    
    private void HandleCreateBtnClick(){
        NetworkTransport.Instance.ConnectTcpToServer(IPAddress.Parse(this.ipInputField.text), int.Parse(this.portInputField.text), () => {
            Debug.Log("Successfully connected to server");
            var packet = new CreateWaitingRoomPacket();
            packet.udpPort = (ushort)NetworkTransport.Instance.SocketUDPPort;
            packet.playerName = playerName.text;
            NetworkTransport.Instance.SendPacketTCP(packet);
            NetworkClientInfoHolder.Instance.playerName = this.playerName.text;
        });
    }

    private void HandleJoinWaitingRoomFailed(JoinWaitingRoomPacket packet){
        NetworkTransport.Instance.Disconnect();
        this.gameObject.SetActive(true);
        UIManager.Instance.UIWaitingRoom.gameObject.SetActive(false);
    }
#endif
}
