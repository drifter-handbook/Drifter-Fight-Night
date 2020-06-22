using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class InputToHostPacket : IGamePacket
{
    public string TypeID { get; set; } = "Input";

    InputData input;
    private class InputData
    {
        public int MoveX = 0;
        public int MoveY = 0;
        public bool Attack1 = false;
        public bool Attack2 = false;
        public bool Attack3 = false;
    }

    public IGamePacket FromData(string json)
    {
        InputData data = JsonConvert.DeserializeObject<InputData>(json);
        return new InputToHostPacket() { input = data };
    }

    public object ToData()
    {
        return input;
    }
}
