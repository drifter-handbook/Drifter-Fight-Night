using System;

public interface INetworkSync
{
    string Type { get; }
    int ID { get; set; }

    void Deserialize(INetworkEntityData data);
    INetworkEntityData Serialize();
}