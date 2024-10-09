using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

using NetworkPlayer = Kigor.Networking.NetworkPlayer;

public class UIManager : MonoBehaviour
{
    public UIJoinRoom UIJoinRoom;
    public UIWaitingRoom UIWaitingRoom;
#if CLIENT_BUILD
    public static UIManager Instance;

    private Camera uiCam;

    private void Awake()
    {
        Instance = this;
        uiCam = Camera.main;

        NetworkTransport.Instance.OnServerCrash += this.ServerCrashCallback;

    }

    private void Start()
    {
        this.UIJoinRoom.Init();
        this.UIWaitingRoom.Init();

        NetworkHandleClient.Instance.OnGameStart += this.GameStartCallback;
        NetworkHandleClient.Instance.OnPlayerLeave += this.LocalPlayerLeaveCallback;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            NetworkPlayer.localPlayer.SendLeaveMessage();
        }
    }

    private void GameStartCallback(Scene loadedScene, NetworkGameRoom room)
    {
        UIWaitingRoom.gameObject.SetActive(false);
        var playerList = room.GetPlayersList();
        foreach (var player in playerList)
        {
            if (player.Name == NetworkClientInfoHolder.Instance.playerName)
            {
                player.SetAsLocalPlayer(true);
                //var cameraController = GameObject.FindObjectOfType<ClientCameraController>();
                //cameraController.SetTargetPlayer(player);
                //(player as FPSPlayer).SetCameraController(cameraController);
                uiCam.gameObject.SetActive(false);
            }
        }
    }

    private void ServerCrashCallback(SocketWrapper socket)
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            UIWaitingRoom.gameObject.SetActive(false);
            UIJoinRoom.gameObject.SetActive(true);
            uiCam.gameObject.SetActive(true);
        });
    }

    private void LocalPlayerLeaveCallback(int id){
        ThreadManager.ExecuteOnMainThread(() =>
        {
            UIWaitingRoom.gameObject.SetActive(false);
            UIJoinRoom.gameObject.SetActive(true);
            uiCam.gameObject.SetActive(true);
        });
    }
#endif
}
