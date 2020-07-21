using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public class StageSelectSyncPacket : IGamePacket
{
    public IPAddress address { get; set; }
    public int port { get; set; }

    public string TypeID { get; set; } = "SSS"; // get rekt noah
    public float Timestamp { get; set; }

    StageSelectSyncData Data;
    private class StageSelectSyncData
    {
    }

    public IGamePacket FromData(string json)
    {
        StageSelectSyncData data = JsonConvert.DeserializeObject<StageSelectSyncData>(json);
        return new StageSelectSyncPacket() { Data = data };
    }

    public object ToData()
    {
        return Data;
    }
}
