using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

public class PingPacket : IGamePacket
{
    public string TypeID { get; set; } = "Ping";
    public float Timestamp { get; set; }

    PingData Time;
    private class PingData
    {
        public float Timestamp;
    }

    public IGamePacket FromData(string json)
    {
        PingData data = JsonConvert.DeserializeObject<PingData>(json);
        return new PingPacket() { Time = data };
    }

    public object ToData()
    {
        return new PingData() { Timestamp = Time.Timestamp };
    }
}
