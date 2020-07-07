using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public class PingPacket : IGamePacket
{
    public IPAddress address { get; set; }
    public int port { get; set; }

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
