using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSyncToHost : MonoBehaviour
{
    NetworkSync networkSync;

    public object this[string key] {
        get {
            if (GameController.Instance.IsHost) { throw new InvalidOperationException("Invalid operation as host."); }
            return NetworkUtils.GetNetworkDataToHost(networkSync.ObjectID)[key];
        }
        set {
            if (GameController.Instance.IsHost) { throw new InvalidOperationException("Invalid operation as host."); }
            INetworkData data = value as INetworkData;
            if (data != null)
            {
                data.Type = data.GetType().Name;
            }
            NetworkUtils.GetNetworkDataToHost(networkSync.ObjectID)[key] = value;
        }
    }
    public object this[string key, int peerId]
    {
        get {
            if (!GameController.Instance.IsHost) { throw new InvalidOperationException("Invalid operation as client."); }
            if (!NetworkUtils.GetNetworkDataFromClient(networkSync.ObjectID, peerId).ContainsKey(key)) { return null; }
            return NetworkUtils.GetNetworkDataFromClient(networkSync.ObjectID, peerId)[key];
        }
        set {
            if (!GameController.Instance.IsHost) { throw new InvalidOperationException("Invalid operation as client."); }
            INetworkData data = value as INetworkData;
            if (data != null)
            {
                data.Type = data.GetType().Name;
            }
            NetworkUtils.GetNetworkDataFromClient(networkSync.ObjectID, peerId)[key] = value;
        }
    }

    void Awake()
    {
        networkSync = GetComponent<NetworkSync>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

