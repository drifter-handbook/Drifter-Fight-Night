using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class SyncToClientPacket : IGamePacket
{
    public string TypeID { get; set; } = "Sync";

    SyncToClientData syncData;
    private class SyncToClientData
    {
    }

    public IGamePacket FromData(string json)
    {
        SyncToClientData data = JsonConvert.DeserializeObject<SyncToClientData>(json);
        return new SyncToClientPacket() { syncData = data };
    }

    public object ToData()
    {
        return syncData;
    }
}
