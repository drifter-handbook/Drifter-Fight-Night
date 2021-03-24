using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTestHost : MonoBehaviour, ISyncHost, INetworkMessageReceiver
{
    NetworkSync sync;
    NetworkSyncToHost syncFromClients;

    bool sentMessage = false;

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        syncFromClients = GetComponent<NetworkSyncToHost>();
        sync["test"] = "Test data.";
    }

    // Update is called once per frame
    void Update()
    {
        if (!sentMessage && GameController.Instance.host.Peers.Count > 0)
        {
            sync.SendNetworkMessage("Message2");
            sentMessage = true;
        }
        foreach (int peerID in GameController.Instance.host.Peers)
        {
            if (syncFromClients["test2", peerID] as string != null)
            {
                Debug.Log($"Test2 data from host: {syncFromClients["test2", peerID] as string}");
            }
        }
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        Debug.Log($"Received message from client: {message.contents as string}");
    }
}
