using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSyncObject
{
    float time = 0f;
    float latency = 0.025f;
    Vector3 oldPos;
    Vector3 targetPos;
    INetworkEntityData data;

    public int ID { get; set; } = NetworkEntityList.NextID;
    public string Type { get; private set; }
    public bool Active { get; private set; }

    // <summary> Creates a default object </summary>
    public NetworkSyncObject()
    {
        this.Type = "DefaultObject";
        this.data = null;
    }

    public NetworkSyncObject(string type, INetworkEntityData data)
    {
        this.Type = type;
        this.data = data;
    }

    // <summary> Prepare data to be sent to other clients </summary>
    public INetworkEntityData Serialize()
    {
        return null;
    }

    // <summary> Read data and return as object </summary>
    public void Deserialize(INetworkEntityData data)
    {

    }
}