using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

public class SyncToClientPacket : IGamePacket
{
    public IPAddress address { get; set; }
    public int port { get; set; }

    public string TypeID { get; set; } = "Sync";
    public float Timestamp { get; set; }

    NetworkEntityConverter converter = new NetworkEntityConverter();

    public SyncToClientData SyncData;
    public class SyncToClientData
    {
        public List<INetworkEntityData> entities = new List<INetworkEntityData>();
        public bool pause;
    }

    public IGamePacket FromData(string json)
    {
        SyncToClientData data = JsonConvert.DeserializeObject<SyncToClientData>(json, converter);
        return new SyncToClientPacket() { SyncData = data };
    }

    public object ToData()
    {
        return SyncData;
    }
}

