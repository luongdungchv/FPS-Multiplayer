using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public class DefaultEncoder : PacketEncoder
    {
        public override PacketData DecodeMessage(string msg)
        {
            return null;
        }

        public override string EncodeData(PacketData data)
        {
            return "asdf";
        }
        
    }
}
