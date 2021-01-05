using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorStateHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;
    int animationLayer = 0;
    Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        sync = GetComponent<NetworkSync>();
        anim = GetComponent<Animator>();
       
    }

    // Update is called once per frame
    void Update()
    {
        sync["animator_state"] = 
            new SyncAnimatorState
            {
                stateHash = anim.GetCurrentAnimatorStateInfo(animationLayer).fullPathHash,
                active = anim.enabled
            };//new SyncAnimatorData() { parameters = parameters };

            //sync.SendNetworkMessage(new SyncAnimatorState() { stateHash = Animator.StringToHash(name), active = anim.enabled});
    }

    public void SetState(string name, int Layer = 0)
    {
        //if(anim.)
        animationLayer = Layer;
        anim.Play(Animator.StringToHash(name),animationLayer);        
        if (GameController.Instance.IsHost)
        {
            //UnityEngine.Debug.Log("MESSAGE SENT: " + Animator.StringToHash(name));
            sync.SendNetworkMessage(new SyncAnimatorState() { stateHash = Animator.StringToHash(name), active = anim.enabled, layer = Layer});
        }
    }
}

public class SyncAnimatorState : INetworkData
{
    public int stateHash;
    public int layer;
    public bool active;
    public string Type { get; set; }
}