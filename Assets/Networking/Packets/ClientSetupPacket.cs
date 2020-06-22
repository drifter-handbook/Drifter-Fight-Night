using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class ClientSetupPacket : IGamePacket
{
    public string TypeID { get; set; } = "Setup";

    public int ID;

    private class ClientSetupData
    {
        public int ID;
    }

    public IGamePacket FromData(string json)
    {
        ClientSetupData data = JsonConvert.DeserializeObject<ClientSetupData>(json);
        return new ClientSetupPacket() { ID = data.ID };
    }

    public object ToData()
    {
        return new ClientSetupData() { ID = ID };
    }
}
