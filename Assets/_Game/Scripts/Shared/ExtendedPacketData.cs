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
            FPS_RECONCILE_PACKET = 16,
            FPS_SHOOT = 17,
            FPS_PLAYER_SHOT = 18,
            FPS_WEAPON_RELOAD = 19,
            FPS_WEAPON_CHANGE = 20,
            FPS_PLAYER_DIE = 21,
            FPS_SERVER_RESPOND_SHOT = 22
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
        public byte damage;
        public override byte[] EncodeData()
        {
            var byteList = new List<byte>();
            byteList.Add((byte)this.PacketType);
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.shootDir.x)));
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.shootDir.y)));
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.shootDir.z)));
            byteList.Add(damage);
            
            byteList.Insert(0, (byte)byteList.Count);

            return byteList.ToArray();
        }

        public override void DecodeMessage(byte[] msg)
        {
            this.shootDir.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 1));
            this.shootDir.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 3));
            this.shootDir.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 5));
            this.damage = msg[7];
        }
    }

    public class FPSPlayerShotPacket : PacketData
    {
        public override PacketType PacketType => PacketType.FPS_PLAYER_SHOT;
        
        public byte playerID;
        public Vector3 hitPos;
        
        public override byte[] EncodeData()
        {
            var byteList = new List<byte>();
            byteList.Add((byte)this.PacketType);
            byteList.Add(playerID);
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.hitPos.x)));
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.hitPos.y)));
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.hitPos.z)));
            byteList.Insert(0, (byte)byteList.Count);
            return byteList.ToArray();
        }

        public override void DecodeMessage(byte[] msg)
        {
            this.playerID = msg[1];
            this.hitPos.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 2));
            this.hitPos.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 4));
            this.hitPos.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 6));
        }
    }

    public class FPSWeaponReloadPacket : PacketData
    {
        public override PacketType PacketType => PacketType.FPS_WEAPON_RELOAD;

        public float duration;
        public ushort sendTimeMili;
        public byte sendTimeSec;
        public byte playerID;

        public override byte[] EncodeData()
        {
            var byteList = new List<byte>();
            byteList.Add((byte)this.PacketType);
            byteList.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(this.duration)));
            byteList.AddRange(BitConverter.GetBytes(this.sendTimeMili));
            byteList.Add(this.sendTimeSec);
            byteList.Add(this.playerID);
            byteList.Insert(0, (byte)byteList.Count);
            return byteList.ToArray();
        }

        public override void DecodeMessage(byte[] msg)
        {
            this.duration = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 1));
            this.sendTimeMili = BitConverter.ToUInt16(msg, 3);
            this.sendTimeSec = msg[5];
        }
    }

    public class FPSWeaponChangePacket : PacketData
    {
        public override PacketType PacketType => PacketType.FPS_WEAPON_CHANGE;
        public WeaponEnum weapon;
        public byte playerID;
        public override byte[] EncodeData()
        {
            return new byte[] { 3, (byte)this.PacketType, (byte)this.weapon, this.playerID };
        }

        public override void DecodeMessage(byte[] msg)
        {
            this.weapon = (WeaponEnum)msg[1];
            this.playerID = msg[2];
        }
    }
    public class FPSPlayerDiePacket : PacketData
    {
        public override PacketType PacketType => PacketType.FPS_PLAYER_DIE;
        public byte playerID;
        public override byte[] EncodeData()
        {
            return new byte[]{2, (byte)this.PacketType, this.playerID};
        }

        public override void DecodeMessage(byte[] msg)
        {
            this.playerID = msg[1];
        }
    }

    public class FPSServerRespondShotPacket : PacketData
    {
        public override PacketType PacketType => PacketType.FPS_SERVER_RESPOND_SHOT;

        public byte playerID;
        public Vector3 endPos;
        
        public override byte[] EncodeData()
        {
            var result = new byte[9];
            result[0] = 8;
            result[1] = (byte)this.PacketType;
            result[2] = this.playerID;
            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.endPos.x)), 0, result, 3, 2);
            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.endPos.y)), 0, result, 5, 2);
            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.endPos.z)), 0, result, 7, 2);   
            return result;
        }

        public override void DecodeMessage(byte[] msg)
        {
            this.playerID = msg[1];
            endPos.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 2));
            endPos.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 4));
            endPos.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 6));
        }
    }
}
