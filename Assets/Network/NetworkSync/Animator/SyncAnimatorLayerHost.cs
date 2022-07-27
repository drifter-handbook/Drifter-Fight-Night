using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorLayerHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;
    Drifter drifter;
    Animator anim;


    // Start is called before the first frame update
    void Awake()
    {
        sync = GetComponent<NetworkSync>();
        drifter = gameObject.transform.parent.gameObject.GetComponent<Drifter>();
        anim = GetComponent<Animator>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameController.Instance.IsOnline) return;
        sync["animator_layer"] = 
        new SyncAnimatorLayer
        {
            //layer = drifter.GetAnimationLayer()
        };
    }

    public void SetLayer(int Layer = 0)
    {
        if(!GameController.Instance.IsOnline) return;
        try
        {
           sync.SendNetworkMessage(new SyncAnimatorLayer() {layer = Layer});
        }
        catch(KeyNotFoundException)
        {
            UnityEngine.Debug.Log("Failed to Sync to layer" + Layer);
        }
        
    }
}

public class SyncAnimatorLayer : INetworkData
{
    public int layer;
    public string Type { get; set; }
}