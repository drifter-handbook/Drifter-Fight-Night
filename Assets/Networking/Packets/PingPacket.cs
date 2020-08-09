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
    public bool Response;

    public class PingData
    {
        public float Timestamp;
        // whether we're sending a ping or this is a response to a ping
        public bool Response = false;
    }

    public IGamePacket FromData(string json)
    {
        PingData data = JsonConvert.DeserializeObject<PingData>(json);
        return new PingPacket() { Timestamp = data.Timestamp, Response = data.Response };
    }

    public object ToData()
    {
        return new PingData() { Timestamp = Timestamp, Response = Response };
    }
}
