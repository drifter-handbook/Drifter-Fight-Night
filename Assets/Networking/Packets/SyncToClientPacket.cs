using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class SyncToClientPacket : IGamePacket
{
    public string TypeID { get; set; } = "Sync";
    public float Timestamp { get; set; }

    public class PlayerData
    {
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public bool facing = false;
    }
    public class ObjectData
    {
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float angle = 0f;
    }

    public SyncToClientData SyncData;
    public class SyncToClientData
    {
        public List<PlayerData> players = new List<PlayerData>();
        public List<ObjectData> objects = new List<ObjectData>();
    }

    public IGamePacket FromData(string json)
    {
        SyncToClientData data = JsonConvert.DeserializeObject<SyncToClientData>(json);
        return new SyncToClientPacket() { SyncData = data };
    }

    public object ToData()
    {
        return SyncData;
    }
}
