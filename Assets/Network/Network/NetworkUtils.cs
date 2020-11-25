using LiteNetLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

public static class NetworkUtils
{
    public static float LerpLatency = 0.05f;

    public static bool IsHost => GameController.Instance.IsHost;

    public static Dictionary<string, object> GetNetworkObjectData(int objectID)
    {
        if (objectID < 0)
        {
            throw new InvalidOperationException("GameObject has not yet been assigned an ObjectID.");
        }
        // fetch host's data
        if (GameController.Instance.IsHost)
        {
            return GameController.Instance.host.data.GetData(objectID);
        }
        // fetch client's data
        else
        {
            return GameController.Instance.client.data.GetData(objectID);
        }
    }
    public static List<NetworkMessage> PopNetworkMessages(int objectID)
    {
        if (objectID < 0)
        {
            throw new InvalidOperationException("GameObject has not yet been assigned an ObjectID.");
        }
        // fetch host's messages from clients
        if (GameController.Instance.IsHost)
        {
            NetworkHost host = GameController.Instance.host;
            List<NetworkMessage> messages = new List<NetworkMessage>();
            foreach (int peerId in host.clientMessages.Keys)
            {
                messages.AddRange(host.clientMessages[peerId].PopMessages(objectID).Select(x => new NetworkMessage()
                {
                    contents = x,
                    peerId = peerId
                }));
            }
            return messages;
        }
        // fetch client's messages from host
        else
        {
            NetworkClient client = GameController.Instance.client;
            if (client == null)
            {
                return new List<NetworkMessage>();
            }
            return client.messages.PopMessages(objectID).Select(x => new NetworkMessage() {
                contents = x,
                peerId = client.netManager.FirstPeer.Id
            }).ToList();
        }
    }
    public static void DestroyNetworkObject(int objectID)
    {
        if (objectID < 0)
        {
            throw new InvalidOperationException("GameObject has not yet been assigned an ObjectID.");
        }
        // destroy host's data
        if (GameController.Instance.IsHost)
        {
            GameController.Instance.host?.data.DestroyData(objectID);
            GameController.Instance.host?.networkObjects?.RemoveNetworkObjectEntry(objectID);
            SendNetworkMessage(0, new DestroyNetworkObjectPacket() { objectID = objectID });
        }
        // destroy client's data
        else
        {
            GameController.Instance.client?.data.DestroyData(objectID);
        }
    }

    public static Dictionary<string, object> GetNetworkDataToHost(int objectID)
    {
        if (!GameController.Instance.IsHost)
        {
            return GameController.Instance.client.dataToHost.GetData(objectID);
        }
        throw new InvalidOperationException("That's not how this works.");
    }

    public static Dictionary<string, object> GetNetworkDataFromClient(int objectID, int peerID)
    {
        if (GameController.Instance.IsHost)
        {
            if (!GameController.Instance.host.clientData.ContainsKey(peerID))
            {
                GameController.Instance.host.clientData[peerID] = new NetworkObjectData();
            }
            return GameController.Instance.host.clientData[peerID].GetData(objectID);
        }
        throw new InvalidOperationException("That's not how this works.");
    }

    public static void SendNetworkMessageToPeer(int peerID, int objectID, object obj, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableSequenced)
    {
        // send message host -> specific client
        if (!GameController.Instance.IsHost)
        {
            throw new InvalidOperationException("Can only use this method as host.");
        }
        NetworkHost host = GameController.Instance.host;
        foreach (NetPeer peer in host.netManager)
        {
            if (peer.Id == peerID)
            {
                peer.Send(host.netPacketProcessor.Write(NetworkMessages.ToPacket(objectID, obj)), deliveryMethod);
            }
        }
    }

    public static void SendNetworkMessage(int objectID, object obj, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableSequenced)
    {
        // send message host -> client
        if (GameController.Instance.IsHost)
        {
            NetworkHost host = GameController.Instance.host;
            host.netManager.SendToAll(host.netPacketProcessor.Write(NetworkMessages.ToPacket(objectID, obj)), deliveryMethod);
        }
        // send message client -> host
        else
        {
            NetworkClient client = GameController.Instance.client;
            client.netManager.SendToAll(client.netPacketProcessor.Write(NetworkMessages.ToPacket(objectID, obj)), deliveryMethod);
        }
    }

    public static T GetNetworkData<T>(object netData) where T : class, INetworkData
    {
        if (netData is JObject)
        {
            try
            {
                T obj = (netData as JObject).ToObject<T>();
                if (obj.Type != typeof(T).Name)
                {
                    return null;
                }
                return obj;
            }
            catch (JsonSerializationException)
            {
            }
        }
        else
        {
            return netData as T;
        }
        return null;
    }

    public static void RegisterChildObject(string networkType, GameObject obj)
    {
        if (GameController.Instance.IsHost)
        {
            GameController.Instance.host.networkObjects.RegisterNetworkObject(NetworkHost.NextObjectID, networkType, obj);
        }
        else
        {
            GameController.Instance.client.networkObjects.RegisterNetworkObject(NetworkClient.NextObjectID, networkType, obj);
        }
    }

    public static byte[] Compress(string dataString)
    {
        byte[] data = Encoding.ASCII.GetBytes(dataString);
        using (var outputStream = new MemoryStream())
        {
            using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gZipStream.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }
    }

    public static string Decompress(byte[] data)
    {
        using (var inputStream = new MemoryStream(data))
        {
            using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    gZipStream.CopyTo(outputStream);
                    return Encoding.ASCII.GetString(outputStream.ToArray());
                }
            }
        }
    }
}
