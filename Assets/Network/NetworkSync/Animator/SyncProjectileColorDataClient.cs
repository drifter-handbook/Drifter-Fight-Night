using System.Collections.Generic;
using UnityEngine;

public class SyncProjectileColorDataClient : MonoBehaviour, ISyncClient, INetworkMessageReceiver
{
    NetworkSync sync;

    SpriteRenderer sprite;

    SyncInt state = new SyncInt();

    int color = 0;

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        sprite = GetComponent<SpriteRenderer>();
    }

    //Update is called once per frame
    void Update()
    {
        try
        {
            state = NetworkUtils.GetNetworkData<SyncInt>(sync["colorInfo"]);

            if(color != state.integerValue) sprite.material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)state.integerValue]);

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

        sprite.material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)state.integerValue]);

    }
}
