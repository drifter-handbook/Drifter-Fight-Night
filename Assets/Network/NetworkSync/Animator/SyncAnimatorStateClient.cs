using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorStateClient : MonoBehaviour, ISyncClient, INetworkMessageReceiver
{
    NetworkSync sync;
    Animator anim;

    SyncAnimatorState state = new SyncAnimatorState();
    SyncAnimatorSpeed speed = new SyncAnimatorSpeed();

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        anim = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        if(!GameController.Instance.IsOnline) return;
        try
        {
            state = NetworkUtils.GetNetworkData<SyncAnimatorState>(sync["animator_state"]);
            speed = NetworkUtils.GetNetworkData<SyncAnimatorSpeed>(sync["animator_speed"]);

            if(anim.GetCurrentAnimatorStateInfo(state.layer).fullPathHash != state.stateHash)
            {
                anim.Play(state.stateHash);
            }
            anim.speed = speed.speed;
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
        if(!GameController.Instance.IsOnline) return;
        state = NetworkUtils.GetNetworkData<SyncAnimatorState>(sync["animator_state"]);
        speed = NetworkUtils.GetNetworkData<SyncAnimatorSpeed>(sync["animator_speed"]);

        if(anim.GetCurrentAnimatorStateInfo(state.layer).fullPathHash != state.stateHash)
            {
                anim.Play(state.stateHash,state.layer);
            }
            anim.speed = speed.speed;
            anim.enabled = state.active;
    }
}
