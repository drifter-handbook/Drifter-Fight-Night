using System.Collections.Generic;
using UnityEngine;

public class SyncColorDataClient : MonoBehaviour, ISyncClient, INetworkMessageReceiver
{
    NetworkSync sync;

    Drifter drifter;

    SyncColorState state = new SyncColorState();

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
            state = NetworkUtils.GetNetworkData<SyncColorState>(sync["playerColor"]);

            if(drifter.GetColor() != state.color)
            {
                drifter.SetColor(state.color);
            }
        }
        catch (KeyNotFoundException)
        {
            UnityEngine.Debug.Log("COLOR MACHINE BROKE");
            // host hasn't sent anything yet
        }
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        state = NetworkUtils.GetNetworkData<SyncColorState>(sync["animator_state"]);

        if(drifter.GetColor() != state.color)
            {
                drifter.SetColor(state.color);
            }
    }
}
