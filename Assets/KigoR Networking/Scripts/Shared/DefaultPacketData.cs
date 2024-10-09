using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public class DefaultPacketData : PacketData
    {
        public override PacketType PacketType => PacketType.JOIN_ROOM;
        
        public Vector3 position;
        public Vector3 lookDir;
        public byte roomID;

        public override void DecodeMessage(byte[] msg)
        {
            throw new System.NotImplementedException();
        }

        public override byte[] EncodeData()
        {
            var byteList = new List<byte>();
            byteList.Add(roomID);
            return null;
        }
    }
}
