using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

public class NoOpPacket : IGamePacket
{
    public string TypeID { get; set; } = "NoOp";

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
