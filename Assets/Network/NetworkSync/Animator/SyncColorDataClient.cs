using System.Collections.Generic;
using UnityEngine;

public class SyncColorDataClient : MonoBehaviour, ISyncClient, INetworkMessageReceiver
{
    NetworkSync sync;

    Drifter drifter;

    SyncInt state = new SyncInt();

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        drifter = GetComponent<Drifter>();
    }
    // Update is called once per frame
    void Update()
    {
        try
        {
            state = NetworkUtils.GetNetworkData<SyncInt>(sync["colorInfo"]);

            if(drifter.GetColor() != state.integerValue) drifter.SetColor(state.integerValue);

        }
        catch (KeyNotFoundException)
        {
            UnityEngine.Debug.Log("COLOR MACHINE BROKE");
            // host hasn't sent anything yet
        }
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        state = NetworkUtils.GetNetworkData<SyncInt>(sync["colorInfo"]);

        if(drifter.GetColor() != state.integerValue)drifter.SetColor(state.integerValue);

    }
}
