using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorLayerHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;
    Drifter drifter;


    // Start is called before the first frame update
    void Awake()
    {
        sync = GetComponent<NetworkSync>();
        drifter = GetComponent<Drifter>();
       
    }

    // Update is called once per frame
    void Update()
    {
        sync["animator_layer"] = 
        new SyncAnimatorLayer
        {
            layer = drifter.GetAnimationLayer()
        };
    }

    public void SetLayer(int Layer = 0)
    {
        //if(anim.)
        try
        {
            //UnityEngine.Debug.Log("MESSAGE SENT: " + Animator.StringToHash(name));
            sync.SendNetworkMessage(new SyncAnimatorLayer() {layer = Layer});
        }
        catch(KeyNotFoundException)
        {
            UnityEngine.Debug.Log("Failed to transition to state: " + name + " on layer: " + Layer);
        }
        
    }
}

public class SyncAnimatorLayer : INetworkData
{
    public int layer;
    public string Type { get; set; }
}