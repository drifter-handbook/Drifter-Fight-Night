using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class CharacterSelectInputPacket : IGamePacket
{
    public string TypeID { get; set; } = "CSI";
    public float Timestamp { get; set; }

    public int Character;
    private class CharacterSelectInputData
    {
        public int Character;
    }

    public IGamePacket FromData(string json)
    {
        CharacterSelectInputData data = JsonConvert.DeserializeObject<CharacterSelectInputData>(json);
        return new CharacterSelectInputPacket() { Character = data.Character };
    }

    public object ToData()
    {
        return new CharacterSelectInputData() { Character = Character };
    }
}
