using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class PlayerInputData
{
    public int MoveX = 0;
    public int MoveY = 0;
    public bool Jump = false;
    public bool Light = false;
    public bool Grab = false;
    public bool Guard = false;
}

public class InputToHostPacket : IGamePacket
{
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
