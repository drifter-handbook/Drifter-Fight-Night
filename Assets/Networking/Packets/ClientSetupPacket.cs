using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

public class ClientSetupPacket : IGamePacket
{
    public IPAddress address { get; set; }
    public int port { get; set; }

    public string TypeID { get; set; } = "Setup";
    public float Timestamp { get; set; }

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
