using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSync : MonoBehaviour
{
    public int ObjectID { get; set; } = -1;
    public string NetworkType = "";

    public Dictionary<string, object> syncData;
    public object this[string key] {
        get { return syncData[key]; }
        set {
            INetworkData data = value as INetworkData;
            if (data != null)
            {
                data.Type = data.GetType().Name;
            }
            syncData[key] = value;
        }
    }

    public void Initialize(int objectID, string networkType)
    {
        ObjectID = objectID;
        NetworkType = networkType;
        syncData = NetworkUtils.GetNetworkObjectData(ObjectID);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (syncData == null)
        {
            throw new InvalidOperationException("Initialize(objectID, networkType) was not called before Start(). Was this object registered with NetworkStartingEntities?");
        }
        this["NetworkType"] = NetworkType;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (NetworkMessage message in NetworkUtils.PopNetworkMessages(ObjectID))
        {
            foreach (INetworkMessageReceiver component in GetComponents<INetworkMessageReceiver>())
            {
                component.ReceiveNetworkMessage(message);
            }
        }
    }

    public void SendNetworkMessage(object obj, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableSequenced)
    {
        NetworkUtils.SendNetworkMessage(ObjectID, obj, deliveryMethod);
    }

    void OnDestroy()
    {
        NetworkUtils.DestroyNetworkObject(ObjectID);
    }
}

public interface ISyncHost { }
public interface ISyncClient { }

public interface INetworkMessageReceiver
{
    void ReceiveNetworkMessage(NetworkMessage message);
}

