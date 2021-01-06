using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorStateClient : MonoBehaviour, ISyncClient, INetworkMessageReceiver
{
    NetworkSync sync;
    Animator anim;

    SyncAnimatorState state = new SyncAnimatorState();

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        anim = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        try
        {
            state = NetworkUtils.GetNetworkData<SyncAnimatorState>(sync["animator_state"]);

            if(anim.GetCurrentAnimatorStateInfo(state.layer).fullPathHash != state.stateHash)
            {

                anim.SetLayerWeight(state.layer == 0?1:0,0);
                anim.SetLayerWeight(state.layer == 0?0:1,1);
                
                UnityEngine.Debug.Log("PLAYING STATE");
                anim.Play(state.stateHash,state.layer);
            }
            anim.enabled = state.active;
        }
        catch (KeyNotFoundException)
        {
            UnityEngine.Debug.Log("KEY NOT FOUND");
            // host hasn't sent anything yet
        }
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        state = NetworkUtils.GetNetworkData<SyncAnimatorState>(sync["animator_state"]);

        if(anim.GetCurrentAnimatorStateInfo(state.layer).fullPathHash != state.stateHash)
            {
                anim.SetLayerWeight(state.layer == 0?0:1,0);
                anim.SetLayerWeight(state.layer == 1?0:1,1);

                anim.Play(state.stateHash,state.layer);
            }
            anim.enabled = state.active;
    }
}
