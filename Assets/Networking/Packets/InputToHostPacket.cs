using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

public class InputToHostPacket : IGamePacket
{
    public IPAddress address { get; set; }
    public int port { get; set; }

    public string TypeID { get; set; } = "Input";
    public float Timestamp { get; set; }

    public PlayerInputData input;

    public IGamePacket FromData(string json)
    {
        PlayerInputData data = JsonConvert.DeserializeObject<PlayerInputData>(json);
        return new InputToHostPacket() { input = data };
    }

    public object ToData()
    {
        return input;
    }
}
