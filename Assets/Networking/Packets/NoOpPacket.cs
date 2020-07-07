using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public class NoOpPacket : IGamePacket
{
    public IPAddress address { get; set; }
    public int port { get; set; }

    public string TypeID { get; set; } = "NoOp";
    public float Timestamp { get; set; }

    private class NoOpData
    {
    }

    public IGamePacket FromData(string json)
    {
        return new NoOpPacket();
    }

    public object ToData()
    {
        return new NoOpData();
    }
}
