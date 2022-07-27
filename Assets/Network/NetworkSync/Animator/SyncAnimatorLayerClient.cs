using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorLayerClient : MonoBehaviour, ISyncClient, INetworkMessageReceiver
{
    NetworkSync sync;
    Drifter drifter;
    Animator anim;

    SyncAnimatorLayer animatorLayers = new SyncAnimatorLayer();

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        drifter = gameObject.transform.parent.gameObject.GetComponent<Drifter>();
        anim = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        if(!GameController.Instance.IsOnline) return;
        
        animatorLayers = NetworkUtils.GetNetworkData<SyncAnimatorLayer>(sync["animator_layer"]);

        //drifter.SetAnimationLayer(animatorLayers.layer);
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        animatorLayers = NetworkUtils.GetNetworkData<SyncAnimatorLayer>(sync["animator_layer"]);
        
        //drifter.SetAnimationLayer(animatorLayers.layer);
    }
}
