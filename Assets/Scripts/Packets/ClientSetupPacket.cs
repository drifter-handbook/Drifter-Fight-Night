using System;
using System.Collections.Generic;

public class ClientSetupPacket : IGamePacket
{
    public string TypeID { get; set; } = "";

    public void LoadBytes()
    {
        throw new NotImplementedException();
    }

    public byte[] ToBytes()
    {
        throw new NotImplementedException();
    }
}