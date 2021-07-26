using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncInputClient : MonoBehaviour, ISyncClient
{
    NetworkSync sync;
    NetworkSyncToHost syncToHost;

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        syncToHost = GetComponent<NetworkSyncToHost>();
    }

    // Update is called once per frame
    void Update()
    {
        syncToHost["input"] = NetworkPlayers.GetInput(GameController.Instance.controls[0]);
    }
}
