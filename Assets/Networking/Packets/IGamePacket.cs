using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using UnityEngine;

public interface IGamePacket
{
    IPAddress address { get; set; }
    int port { get; set; }

    // unique packet identifier
    string TypeID { get; }

    // timestamp
    float Timestamp { get; set; }

    // create a data object that represents this packet
    object ToData();

    // create a new game packet with this data
    IGamePacket FromData(string json);
}

public static class GamePacketUtils
{
    const string DELIMITER = "|";

    static List<IGamePacket> types = new List<IGamePacket>() {
        new NoOpPacket(),
        new ClientSetupPacket(),
        new InputToHostPacket(),
        new SyncToClientPacket(),
        new PingPacket(),
        new CharacterSelectInputPacket(),
        new CharacterSelectSyncPacket()
    };
    static Dictionary<string, IGamePacket> namedTypes = new Dictionary<string, IGamePacket>();
    static GamePacketUtils()
    {
        foreach (IGamePacket type in types)
        {
            namedTypes[type.TypeID] = type;
        }
    }

    static byte[] Compress(string dataString)
    {
        byte[] data = Encoding.ASCII.GetBytes(dataString);
        using (var outputStream = new MemoryStream())
        {
            using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gZipStream.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }
    }

    static string Decompress(byte[] data)
    {
        using (var inputStream = new MemoryStream(data))
        {
            using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    gZipStream.CopyTo(outputStream);
                    return Encoding.ASCII.GetString(outputStream.ToArray());
                }
            }
        }
    }

    // convert packet to bytes
    public static byte[] Serialize(IGamePacket packet)
    {
        return Compress(packet.TypeID + DELIMITER + Time.time + DELIMITER + JsonConvert.SerializeObject(packet.ToData()));
    }
    // create packet from bytes
    public static IGamePacket Deserialize(byte[] data)
    {
        string[] packet = Decompress(data).Split(new string[] { DELIMITER }, 3, StringSplitOptions.None);
        if (namedTypes.ContainsKey(packet[0]))
        {
            IGamePacket gamePacket = namedTypes[packet[0]].FromData(packet[2]);
            gamePacket.Timestamp = float.Parse(packet[1]);
            return gamePacket;
        }
        return null;
    }
}
