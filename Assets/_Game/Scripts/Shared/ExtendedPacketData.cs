using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kigor.Networking;
using System;

namespace Kigor.Networking
{
    public partial class PacketType
    {
        public static PacketType 
            FPS_INPUT_PACKET = 15,
            FPS_RECONCILE_PACKET = 16
        ;
    }

    public class FPSInputPacket : PacketData
    {
        public override PacketType PacketType => PacketType.FPS_INPUT_PACKET;

        public Vector2 movement;
        public bool jump;
        public bool shoot;
        public byte tick;
        public Vector2 moveDir;
        public float cameraAngle;

        public override void DecodeMessage(byte[] msg)
        {
            movement.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 1));
            movement.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 3));
            jump = msg[5] == 1;
            shoot = msg[6] == 1;
            tick = msg[7];

            moveDir.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 8));
            cameraAngle = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 10));
            moveDir.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 12));             
        }

        public override byte[] EncodeData()
        {
            var listByte = new List<byte>();

            listByte.Add((byte)this.PacketType);
            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(movement.x)));
            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(movement.y)));
            listByte.Add((byte)(jump ? 1 : 0));
            listByte.Add((byte)(shoot ? 1 : 0));
            listByte.Add(tick);
            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(moveDir.x)));
            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(cameraAngle)));
            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(moveDir.y)));
            return listByte.ToArray();
        }
    }

    public class FPSReconcilePacket : PacketData
    {
        public override PacketType PacketType => PacketType.FPS_RECONCILE_PACKET;

        public FPSPlayerState playerState;
        public byte tick;

        public override void DecodeMessage(byte[] msg)
        {
            playerState.position.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 1));
            playerState.position.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 3));
            playerState.position.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 5));
            
            playerState.horizontalRotation = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 7));
            playerState.verticalRotation = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 9));

            this.tick = msg[11];
        }

        public override byte[] EncodeData()
        {
            var listBytes = new List<byte>();
            listBytes.Add((byte)this.PacketType);

            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.position.x)));
            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.position.y)));
            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.position.z)));

            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.horizontalRotation)));
            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.verticalRotation)));

            listBytes.Add(tick);

            return listBytes.ToArray();
        }
    }

    public class FPSShootPacket : PacketData
    {
        public override PacketType PacketType => PacketType.FPS_SHOOT;

        public Vector3 shootDir;
        
        public override byte[] EncodeData()
        {
            var byteList = new List<byte>();
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.shootDir.x)));
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.shootDir.y)));
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.shootDir.z)));
            
            byteList.Insert(0, (byte)byteList.Count);

            return byteList.ToArray();
        }

        public override void DecodeMessage(byte[] msg)
        {
            this.shootDir.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 0));
            this.shootDir.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 2));
            this.shootDir.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 4));
        }
    }
}
