using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorStateHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;

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
                stateHash = anim.GetCurrentAnimatorStateInfo(0).fullPathHash,
                active = anim.enabled
            };//new SyncAnimatorData() { parameters = parameters };

            //sync.SendNetworkMessage(new SyncAnimatorState() { stateHash = Animator.StringToHash(name), active = anim.enabled});
    }

    public void SetState(string name)
    {
        //if(anim.)
        anim.Play(Animator.StringToHash(name));
        if (GameController.Instance.IsHost)
        {
            //UnityEngine.Debug.Log("MESSAGE SENT: " + Animator.StringToHash(name));
            sync.SendNetworkMessage(new SyncAnimatorState() { stateHash = Animator.StringToHash(name), active = anim.enabled});
        }
    }
}

public class SyncAnimatorState : INetworkData
{
    public int stateHash;
    public bool active;
    public string Type { get; set; }
}