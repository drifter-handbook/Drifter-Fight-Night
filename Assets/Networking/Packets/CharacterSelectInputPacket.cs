using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

public class CharacterSelectInputPacket : IGamePacket
{
    public IPAddress address { get; set; }
    public int port { get; set; }

    public string TypeID { get; set; } = "CSI";
    public float Timestamp { get; set; }

    public CharacterSelectState CharacterSelect;

    public IGamePacket FromData(string json)
    {
        CharacterSelectState data = JsonConvert.DeserializeObject<CharacterSelectState>(json);
        return new CharacterSelectInputPacket() { CharacterSelect = data };
    }

    public object ToData()
    {
        return CharacterSelect;
    }
}
