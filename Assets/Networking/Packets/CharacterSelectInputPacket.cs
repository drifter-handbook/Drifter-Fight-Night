using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class CharacterSelectInputPacket : IGamePacket
{
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
