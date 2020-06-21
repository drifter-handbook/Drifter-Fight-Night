using System;
using System.Collections.Generic;

public interface IGamePacket
{
    string TypeID { get; }
    byte[] ToBytes();
    void LoadBytes();
}
