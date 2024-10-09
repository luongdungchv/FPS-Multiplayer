using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public abstract class PacketEncoder
    {
        public abstract string EncodeData(PacketData data);
        public abstract PacketData DecodeMessage(string msg);
    }
}
