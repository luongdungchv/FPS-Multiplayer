using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Kigor.Networking;
using UnityEngine.UI;

public class UIWaitingRoom : MonoBehaviour
{
    [SerializeField] private GameObject playerTextPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private Button btnLeave, btnReady, btnStart;
    [SerializeField] private TMP_Text textRoomID;
    [SerializeField] private Camera camera;
#if CLIENT_BUILD
    private List<GameObject> spawnedPlayerTextList;

    private UIManager uiManager => UIManager.Instance;
    private Transform displayer;
    private string playerName => NetworkClientInfoHolder.Instance.playerName;

    public void Init()
    {
        NetworkHandleClient.Instance.OnWaitingRoomPacketReceived += this.WaitingRoomStateCallback;

        this.spawnedPlayerTextList = new List<GameObject>();

        this.btnLeave.onClick.AddListener(this.LeaveBtnClick);
        this.btnStart.onClick.AddListener(this.StartBtnClick);
        this.btnReady.onClick.AddListener(this.ReadyBtnClick);
    }

    private void WaitingRoomStateCallback(WaitingRoomStatePacket packet)
    {
        try
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                this.ClearList();
                if (!uiManager.UIWaitingRoom.gameObject.activeSelf)
                {
                    uiManager.UIWaitingRoom.gameObject.SetActive(true);
                    uiManager.UIJoinRoom.gameObject.SetActive(false);
                }

                this.btnStart.gameObject.SetActive(this.playerName == packet.playerNames[0]);
                this.btnReady.gameObject.SetActive(this.playerName != packet.playerNames[0]);

                for(int i = 0; i < packet.playerNames.Count; i++)
                {
                    
                    var name = packet.playerNames[i];
                    var ready = packet.readyStates[i];
                    var nameHolder = Instantiate(playerTextPrefab);
                    if(name == this.playerName) displayer = nameHolder.transform;

                    nameHolder.transform.SetParent(container);
                    nameHolder.transform.localScale = Vector3.one;
                    spawnedPlayerTextList.Add(nameHolder);

                    nameHolder.transform.GetChild(0).GetComponent<TMP_Text>().text = name;
                    
                    Debug.Log(name == playerName);
                    
                    nameHolder.GetComponent<Image>().color = name == this.playerName ? Color.yellow : Color.white;
                    nameHolder.transform.GetChild(1).gameObject.SetActive(ready);
                }

                this.textRoomID.text = packet.roomID.ToString();
            });
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    

    private void ClearList()
    {
        spawnedPlayerTextList.ForEach(x => Destroy(x.gameObject));
        spawnedPlayerTextList.Clear();
    }

    private void LeaveBtnClick()
    {
        var packet = new LeaveWaitingRoomPacket();
        NetworkTransport.Instance.SendPacketTCP(packet, () =>
        {
            
            NetworkTransport.Instance.Disconnect(() =>
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    uiManager.UIWaitingRoom.gameObject.SetActive(false);
                    uiManager.UIJoinRoom.gameObject.SetActive(true);
                    NetworkClientInfoHolder.Instance.playerName = "";
                });
            });

        });
    }
    private void StartBtnClick(){
        var packet = new StartGamePacket();
        packet.rule = GameRule.FREE_FOR_ALL;
        NetworkTransport.Instance.SendPacketTCP(packet, () => {
            var sceneName = packet.mapName;
            //TODO: Scene loading
        });
    }
    private void ReadyBtnClick(){
        var packet = new ReadyPacket();
        packet.ready = !this.displayer.GetChild(1).gameObject.activeSelf;
        Debug.Log(packet.ready);
        NetworkTransport.Instance.SendPacketTCP(packet, () => {
            //TODO: State updating
        });
    }
#endif
}
