using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public interface IGamePacket
{
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
        new CharacterSelectSyncPacket(),
        new StageSelectSyncPacket()
    };
    static Dictionary<string, IGamePacket> namedTypes = new Dictionary<string, IGamePacket>();
    static GamePacketUtils()
    {
        foreach (IGamePacket type in types)
        {
            namedTypes[type.TypeID] = type;
        }
    }

    // convert packet to bytes
    public static byte[] Serialize(IGamePacket packet)
    {
        return Encoding.ASCII.GetBytes(packet.TypeID + DELIMITER + Time.time + DELIMITER + JsonConvert.SerializeObject(packet.ToData()));
    }
    // create packet from bytes
    public static IGamePacket Deserialize(byte[] data)
    {
        string[] packet = Encoding.ASCII.GetString(data).Split(new string[] { DELIMITER }, 3, StringSplitOptions.None);
        if (namedTypes.ContainsKey(packet[0]))
        {
            IGamePacket gamePacket = namedTypes[packet[0]].FromData(packet[2]);
            gamePacket.Timestamp = float.Parse(packet[1]);
            return gamePacket;
        }
        return null;
    }
}
