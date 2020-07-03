using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

public class CharacterSelectSyncPacket : IGamePacket
{
    public string TypeID { get; set; } = "CSS"; // get rekt noah
    public float Timestamp { get; set; }

    CharacterSelectSyncData Data;
    private class CharacterSelectSyncData
    {
    }

    public IGamePacket FromData(string json)
    {
        CharacterSelectSyncData data = JsonConvert.DeserializeObject<CharacterSelectSyncData>(json);
        return new CharacterSelectSyncPacket() { Data = data };
    }

    public object ToData()
    {
        return Data;
    }
}
