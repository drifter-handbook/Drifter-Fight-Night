using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncInputHost : MonoBehaviour, ISyncHost
{
    NetworkSyncToHost syncFromClients;

    // Start is called before the first frame update
    void Start()
    {
        syncFromClients = GetComponent<NetworkSyncToHost>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
