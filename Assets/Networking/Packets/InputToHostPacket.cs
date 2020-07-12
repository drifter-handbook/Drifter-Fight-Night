using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

public class PlayerInputData : ICloneable
{
    public int MoveX = 0;
    public int MoveY = 0;
    public bool Jump = false;
    public bool Light = false;
    public bool Special = false;
    public bool Grab = false;
    public bool Guard = false;

    public object Clone()
    {
        return new PlayerInputData() {
            MoveX = MoveX,
            MoveY = MoveY,
            Jump = Jump,
            Light = Light,
            Special = Special,
            Grab = Grab,
            Guard = Guard
        };
    }

    public void CopyFrom(PlayerInputData data)
    {
        MoveX = data.MoveX;
        MoveY = data.MoveY;
        Jump = data.Jump;
        Light = data.Light;
        Special = data.Special;
        Grab = data.Grab;
        Guard = data.Guard;
    }
}

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
