using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class SyncToClientPacket : IGamePacket
{
    public string TypeID { get; set; } = "Sync";

    private class PlayerData
    {
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public int facing = 0;
    }
    private class ObjectData
    {
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float angle = 0f;
    }

    SyncToClientData syncData;
    private class SyncToClientData
    {
        public List<PlayerData> players = new List<PlayerData>();
        public List<ObjectData> objects = new List<ObjectData>();
    }

    public IGamePacket FromData(string json)
    {
        SyncToClientData data = JsonConvert.DeserializeObject<SyncToClientData>(json);
        return new SyncToClientPacket() { syncData = data };
    }

    public object ToData()
    {
        return syncData;
    }
}
