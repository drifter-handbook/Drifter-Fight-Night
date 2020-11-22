using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectData
{
    public Dictionary<int, Dictionary<string, object>> data = new Dictionary<int, Dictionary<string, object>>();

    public Dictionary<string, object> GetData(int objectID)
    {
        if (!data.ContainsKey(objectID))
        {
            data[objectID] = new Dictionary<string, object>();
        }
        return data[objectID];
    }
    public void DestroyData(int objectID)
    {
        data.Remove(objectID);
    }

    public void SyncFromPacket(NetworkObjectDataPacket packet)
    {
        Dictionary<int, Dictionary<string, object>> newData =
            JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, object>>>(NetworkUtils.Decompress(packet.data));
        // sync keys
        foreach (int objectID in newData.Keys)
        {
            if (!data.ContainsKey(objectID))
            {
                data[objectID] = new Dictionary<string, object>();
            }
            foreach (string field in newData[objectID].Keys)
            {
                object value = newData[objectID][field];
                if (value is double)
                {
                    value = (float)value;
                }
                if (value is long)
                {
                    value = (int)value;
                }
                data[objectID][field] = value;
            }
        }
    }

    public NetworkObjectDataPacket ToPacket()
    {
        return new NetworkObjectDataPacket() { data = NetworkUtils.Compress(JsonConvert.SerializeObject(data)) };
    }
}

public class NetworkObjectDataPacket
{
    public byte[] data { get; set; }
}