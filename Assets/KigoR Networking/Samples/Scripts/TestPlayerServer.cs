using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kigor.Networking;
using UnityEngine;
using UnityEngine.Events;

public partial class TestPlayer : Kigor.Networking.NetworkPlayer
{
#if SERVER_BUILD
    protected void TickUpdate()
    {
        //Debug.Log("test");
    }

    public override void Initialize(SocketWrapper socket, NetworkGameRoom room, int id)
    {
        base.Initialize(socket, room, id);

        this.msgHandler.Add(PacketType.PLAYER_STATE, this.HandlePlayerStatePacket);
    }

    private void HandlePlayerStatePacket(byte[] data)
    {
        var packet = new PlayerStatePacket();
        var strList = data.Select(x => x.ToString()).ToArray();

        try
        {
            packet.DecodeMessage(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        ThreadManager.ExecuteOnMainThread(() =>
        {
            transform.position = packet.position;
            transform.eulerAngles = packet.rotation;
        });
    }
#endif
}
