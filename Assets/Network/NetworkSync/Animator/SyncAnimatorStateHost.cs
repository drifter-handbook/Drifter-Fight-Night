using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorStateHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;
    int animationLayer = 0;
    float animationSpeed = 1;
    Animator anim;
    string lastSentState = "";

    // Start is called before the first frame update
    void Awake()
    {
        sync = GetComponent<NetworkSync>();
        anim = GetComponent<Animator>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if(lastSentState != "" && anim.GetCurrentAnimatorStateInfo(animationLayer).shortNameHash != Animator.StringToHash(lastSentState)){
            //UnityEngine.Debug.Log("STATES DESYNCED! PLAYING STATE: " + lastSentState);
            SetState(lastSentState,animationLayer);
            lastSentState = "";
        }

        //UnityEngine.Debug.Log( anim.GetCurrentAnimatorStateInfo(animationLayer).shortNameHash + "   :  " + Animator.StringToHash(lastSentState));
        if(!GameController.Instance.IsOnline) return;
        sync["animator_state"] = 
            new SyncAnimatorState
            {
                stateHash = anim.GetCurrentAnimatorStateInfo(animationLayer).fullPathHash,
                active = anim.enabled
            };//new SyncAnimatorData() { parameters = parameters };

        sync["animator_speed"] = 
            new SyncAnimatorSpeed
            {
                speed = animationSpeed
            };    

            //sync.SendNetworkMessage(new SyncAnimatorState() { stateHash = Animator.StringToHash(name), active = anim.enabled});
    }

    public void SetState(string name, int Layer = 0)
    {
        //if(anim.)
        try
        {
           animationLayer = Layer;
           anim.Play(Animator.StringToHash(name),animationLayer);
           lastSentState = name;        
           if (GameController.Instance.IsHost && GameController.Instance.IsOnline)
            {
            //UnityEngine.Debug.Log("MESSAGE SENT: " + Animator.StringToHash(name));
            sync.SendNetworkMessage(new SyncAnimatorState() { stateHash = Animator.StringToHash(name), active = anim.enabled, layer = Layer});
            } 
        }
        catch(KeyNotFoundException)
        {
            UnityEngine.Debug.Log("Failed to transition to state: " + name + " on layer: " + Layer);
        }
        
    }

    public void SetSpeed(float speed)
    {
        animationSpeed = speed;
        anim.speed = speed;
        if (GameController.Instance.IsHost && GameController.Instance.IsOnline)
        {
        //UnityEngine.Debug.Log("MESSAGE SENT: " + Animator.StringToHash(name));
            sync.SendNetworkMessage(new SyncAnimatorSpeed() { speed = animationSpeed}); 
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

public class SyncAnimatorSpeed : INetworkData
{
    public float speed;
    public string Type { get; set; }
}