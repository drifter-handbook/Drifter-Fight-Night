﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

public interface IGamePacket
{
    // unique packet identifier
    string TypeID { get; }

    // create a data object that represents this packet
    object ToData();

    // create a new game packet with this data
    IGamePacket FromData(string json);
}

public static class GamePacketUtils
{
    const string DELIMITER = "|";

    static List<IGamePacket> types = new List<IGamePacket>() { new NoOpPacket(), new ClientSetupPacket() };
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
        return Encoding.ASCII.GetBytes(packet.TypeID + DELIMITER + JsonConvert.SerializeObject(packet.ToData()));
    }
    // create packet from bytes
    public static IGamePacket Deserialize(byte[] data)
    {
        string[] packet = Encoding.ASCII.GetString(data).Split(new string[] { DELIMITER }, 1, StringSplitOptions.None);
        if (namedTypes.ContainsKey(packet[0]))
        {
            return namedTypes[packet[0]].FromData(packet[1]);
        }
        return null;
    }
}
