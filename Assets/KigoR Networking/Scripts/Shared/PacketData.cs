using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Text;
using System.Linq;
using System.Net;
using System.Data.SqlTypes;

namespace Kigor.Networking
{
    public abstract class PacketData
    {
        public abstract PacketType PacketType { get; }
        public abstract byte[] EncodeData();
        public abstract void DecodeMessage(byte[] msg);
    }

    public class JoinWaitingRoomPacket : PacketData
    {
        public override PacketType PacketType => PacketType.JOIN_WAITING_ROOM;

        public string name = "0";
        public short roomID;
        public bool state;
        public ushort udpPort;

        public override void DecodeMessage(byte[] msg)
        {
            this.roomID = BitConverter.ToInt16(msg, 1);
            this.udpPort = BitConverter.ToUInt16(msg, 4);
            this.name = Encoding.ASCII.GetString(msg, 6, msg.Length - 6);
            this.state = msg[3] == 1;
        }

        public override byte[] EncodeData()
        {
            var listByte = new List<byte>();
            listByte.Add((byte)PacketType);
            listByte.AddRange(BitConverter.GetBytes(roomID));
            listByte.Add((byte)(state ? 1 : 0));
            listByte.AddRange(BitConverter.GetBytes(udpPort));
            listByte.AddRange(Encoding.ASCII.GetBytes(name));
            listByte.Insert(0, (byte)listByte.Count);
            return listByte.ToArray();
        }
    }

    public class DisconnectPacket : PacketData
    {
        public override PacketType PacketType => PacketType.DISCONNECT;

        public override void DecodeMessage(byte[] msg)
        {

        }

        public override byte[] EncodeData()
        {
            var data = new byte[2];
            data[0] = 1;
            data[1] = (byte)PacketType;
            return data;
        }
    }

    public class LeaveWaitingRoomPacket : PacketData
    {
        public override PacketType PacketType => PacketType.LEAVE_WAITING_ROOM;

        public override void DecodeMessage(byte[] msg)
        {

        }

        public override byte[] EncodeData()
        {
            return new byte[] { 1, (byte)PacketType };
        }
    }

    public class WaitingRoomStatePacket : PacketData
    {
        public override PacketType PacketType => PacketType.WAITING_ROOM_STATE;

        public List<string> playerNames;
        public List<bool> readyStates;
        public short roomID;

        public WaitingRoomStatePacket(List<string> playerNames, short roomID)
        {
            this.playerNames = playerNames;
            this.roomID = roomID;
        }
        public WaitingRoomStatePacket()
        {
            this.playerNames = new List<string>();
            this.readyStates = new List<bool>();
        }

        public override void DecodeMessage(byte[] msg)
        {
            this.playerNames = new List<string>();
            var readBytes = new List<byte>();
            string test = "";
            foreach (var i in msg)
            {
                test += i.ToString() + " ";
            }

            this.roomID = BitConverter.ToInt16(msg, 1);

            try
            {
                for (int i = 3; i < msg.Length; i++)
                {
                    if (msg[i] == 254)
                    {
                        var readName = Encoding.ASCII.GetString(readBytes.ToArray(), 0, readBytes.Count - 1);
                        var ready = readBytes[readBytes.Count - 1] == 1 ? true : false;
                        playerNames.Add(readName);
                        readyStates.Add(ready);
                        readBytes.Clear();
                        continue;
                    }
                    readBytes.Add(msg[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }


        }

        public override byte[] EncodeData()
        {
            var byteList = new List<byte>();
            byteList.Add((byte)this.PacketType);
            byteList.AddRange(BitConverter.GetBytes(this.roomID));
            for (int i = 0; i < playerNames.Count; i++)
            {
                var playerName = playerNames[i];
                var encodedName = Encoding.ASCII.GetBytes(playerName);
                Debug.Log(Encoding.ASCII.GetString(encodedName));
                byteList.AddRange(encodedName);
                byteList.Add((byte)(readyStates[i] ? 1 : 0));
                byteList.Add(254);
            }
            byteList.Insert(0, (byte)byteList.Count);
            return byteList.ToArray();
        }
    }

    public class CreateWaitingRoomPacket : PacketData
    {
        public override PacketType PacketType => PacketType.CREATE_WAITING_ROOM;

        public string playerName;
        public ushort udpPort;

        public override void DecodeMessage(byte[] msg)
        {
            this.udpPort = BitConverter.ToUInt16(msg, 1);
            this.playerName = Encoding.ASCII.GetString(msg, 3, msg.Length - 3);
        }

        public override byte[] EncodeData()
        {
            var byteList = new List<byte>();
            byteList.Add((byte)PacketType);
            byteList.AddRange(BitConverter.GetBytes(this.udpPort));
            byteList.AddRange(Encoding.ASCII.GetBytes(this.playerName));
            byteList.Insert(0, (byte)byteList.Count);
            return byteList.ToArray();
        }
    }

    public class ReadyPacket : PacketData
    {
        public override PacketType PacketType => PacketType.READY;
        public bool ready;

        public override void DecodeMessage(byte[] msg)
        {
            this.ready = msg[1] == 1 ? true : false;
        }

        public override byte[] EncodeData()
        {
            var encodedReady = (byte)(ready ? 1 : 0);
            return new byte[] { 2, (byte)PacketType, encodedReady };
        }
    }

    public class StartGamePacket : PacketData
    {
        public override PacketType PacketType => PacketType.START_GAME;

        public string mapName = "0";
        public List<string> playerNameList;
        public GameRule rule;

        public override void DecodeMessage(byte[] msg)
        {
            var readStrings = new List<string>();
            var readBytes = new List<byte>();

            for (int i = 2; i < msg.Length; i++)
            {
                if (msg[i] == 254)
                {
                    var data = Encoding.ASCII.GetString(readBytes.ToArray());
                    readStrings.Add(data);
                    readBytes.Clear();
                    continue;
                }
                readBytes.Add(msg[i]);
            }

            this.mapName = readStrings[0];
            readStrings.RemoveAt(0);
            this.playerNameList = readStrings;

            this.rule = (GameRule)msg[1];
        }

        public override byte[] EncodeData()
        {
            var byteList = new List<byte>();
            byteList.Add((byte)PacketType);
            byteList.Add((byte)this.rule);
            byteList.AddRange(Encoding.ASCII.GetBytes(this.mapName));
            byteList.Add(254);
            if (this.playerNameList != null)
                foreach (var name in this.playerNameList)
                {
                    byteList.AddRange(Encoding.ASCII.GetBytes(name));
                    byteList.Add(254);
                }
            byteList.Insert(0, (byte)byteList.Count);
            return byteList.ToArray();
        }
    }

    

    public class UDPConnectionInfoPacket : PacketData
    {
        public override PacketType PacketType => PacketType.UDP_CONNECTION_INFO;
        public ushort port;

        public override void DecodeMessage(byte[] msg)
        {
            this.port = BitConverter.ToUInt16(msg, 1);
        }

        public override byte[] EncodeData()
        {
            var listBytes = new List<byte>();
            listBytes.Add((byte)this.PacketType);
            listBytes.AddRange(BitConverter.GetBytes(this.port));
            listBytes.Insert(0, (byte)listBytes.Count);
            return listBytes.ToArray();
        }
    }

    public class PlayerStatePacket : PacketData
    {
        public override PacketType PacketType => PacketType.PLAYER_STATE;

        public Vector3 position, rotation;

        public override void DecodeMessage(byte[] msg)
        {
            position.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 1));
            position.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 3));
            position.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 5));

            rotation.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 7));
            rotation.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 9));
            rotation.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 11));
        }

        public override byte[] EncodeData()
        {
            var listBytes = new byte[13];

            listBytes[0] = (byte)PacketType;

            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.position.x)), 0 ,listBytes, 1, 2);
            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.position.y)), 0 ,listBytes, 3, 2);
            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.position.z)), 0 ,listBytes, 5, 2);

            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.rotation.x)), 0 ,listBytes, 7, 2);
            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.rotation.y)), 0 ,listBytes, 9, 2);
            Array.Copy(BitConverter.GetBytes(Mathf.FloatToHalf(this.rotation.z)), 0 ,listBytes, 11, 2);
            
            return listBytes;
        }
    }
    public class RoomStatePacket : PacketData
    {
        public override PacketType PacketType => PacketType.ROOM_STATE;

        public List<Vector3> playerPositionList, playerRotationList;
        public List<int> playerIDList;

        public RoomStatePacket(){
            this.playerPositionList = new List<Vector3>();
            this.playerRotationList = new List<Vector3>();
            this.playerIDList = new List<int>();
        }

        public override void DecodeMessage(byte[] msg)
        {
            var processor = new PlayerStatePacket();
            for(int i = 1; i < msg.Length; i += 13){
                var d = new byte[13];
                Array.Copy(msg, i, d, 0, 13);
                processor.DecodeMessage(d);
                
                playerIDList.Add(d[0]);
                playerPositionList.Add(processor.position);
                playerRotationList.Add(processor.rotation);
            }
        }

        public override byte[] EncodeData()
        {
            var result = new byte[1 + playerPositionList.Count * 13];

            var processor = new PlayerStatePacket();
            for(int i = 0; i < playerPositionList.Count; i++){
                processor.position = playerPositionList[i];
                processor.rotation = playerRotationList[i];
                var data = processor.EncodeData();

                var index = 1 + i * 13;
                result[index] = (byte)playerIDList[i];
                Array.Copy(data, 1, result, index + 1, 12);
            }
            result[0] = (byte)PacketType;
            return result;
        }

        public byte[] EncodeDataTCP(){
            var result = new byte[2 + playerPositionList.Count * 13];

            var processor = new PlayerStatePacket();
            for(int i = 0; i < playerPositionList.Count; i++){
                processor.position = playerPositionList[i];
                processor.rotation = playerRotationList[i];
                var data = processor.EncodeData();

                var index = 2 + i * 13;
                result[index] = (byte)playerIDList[i];
                Array.Copy(data, 1, result, index + 1, 12);
            }
            result[1] = (byte)PacketType;
            result[0] = (byte)(result.Length - 1);
            return result;
        }
    }

    public class PlayerLeavePacket : PacketData
    {
        public override PacketType PacketType => PacketType.PLAYER_LEAVE;

        public byte playerID;

        public override void DecodeMessage(byte[] msg)
        {
            playerID = msg[1];
        }

        public override byte[] EncodeData()
        {
            return new byte[]{2, (byte)this.PacketType, this.playerID};
        }
    }

    public class InputPacket : PacketData
    {
        public override PacketType PacketType => PacketType.INPUT;

        public Vector2 movement;
        public bool jump;
        public bool shoot;
        public byte tick;
        public Vector2 lookDir, rightDir;

        public override void DecodeMessage(byte[] msg)
        {
            movement.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 1));
            movement.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 3));
            jump = msg[5] == 1;
            shoot = msg[6] == 1;
            tick = msg[7];

            lookDir.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 8));
            lookDir.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 10));

            rightDir.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 12));
            rightDir.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 14));
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

            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(lookDir.x)));
            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(lookDir.y)));

            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(rightDir.x)));
            listByte.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(rightDir.y)));

            return listByte.ToArray();
        }
    }

    public class LocalPlayerStatePacket : PacketData
    {
        public override PacketType PacketType => PacketType.LOCAL_PLAYER_STATE;

        public PlayerState playerState;
        public byte tick;

        public override void DecodeMessage(byte[] msg)
        {
            playerState.position.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 1));
            playerState.position.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 3));
            playerState.position.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 5));
            
            playerState.rotation.x = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 7));
            playerState.rotation.y = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 9));
            playerState.rotation.z = Mathf.HalfToFloat(BitConverter.ToUInt16(msg, 11));

            this.tick = msg[13];
        }

        public override byte[] EncodeData()
        {
            var listBytes = new List<byte>();
            listBytes.Add((byte)this.PacketType);

            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.position.x)));
            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.position.y)));
            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.position.z)));

            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.rotation.x)));
            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.rotation.y)));
            listBytes.AddRange(BitConverter.GetBytes(Mathf.FloatToHalf(playerState.rotation.z)));

            listBytes.Add(tick);

            return listBytes.ToArray();
        }
    }

    public partial class PacketType : IEquatable<PacketType>
    {
        private int value;
        public int Value => this.value;
        public PacketType(int value) => this.value = value;
        #region Operators_Overloading
        public static implicit operator PacketType(int value) => new(value);
        public static implicit operator PacketType(byte value) => new(value);
        public static implicit operator PacketType(short value) => new(value);
        public static explicit operator int(PacketType packetType) => packetType.value;

        public static bool operator ==(PacketType packetType1, PacketType packetType2){
            return packetType1.value == packetType2.value;
        }
        public static bool operator !=(PacketType packetType1, PacketType packetType2){
            return packetType1.value != packetType2.value;
        }

        public bool Equals(PacketType other)
        {
            return other.Value == this.value;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as PacketType);
        }
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
        #endregion

        public static PacketType
            JOIN_WAITING_ROOM = 0,
            JOIN_ROOM = 1,
            DISCONNECT = 2,
            LEAVE_WAITING_ROOM = 3,
            WAITING_ROOM_STATE = 4,
            CREATE_WAITING_ROOM = 5,
            READY = 6,
            START_GAME = 7,
            UDP_CONNECTION_INFO = 8,
            PLAYER_STATE = 9,
            ROOM_STATE = 10,
            INIT_ROOM = 11,
            PLAYER_LEAVE = 12,
            INPUT = 13,
            LOCAL_PLAYER_STATE = 14;
    }
}
