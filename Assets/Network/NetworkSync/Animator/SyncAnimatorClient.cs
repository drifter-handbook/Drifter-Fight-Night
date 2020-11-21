using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorClient : MonoBehaviour, ISyncClient, INetworkMessageReceiver
{
    NetworkSync sync;

    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        anim = GetComponent<Animator>();
    }

    void SetAnimatorParameterValue(AnimatorControllerParameterType type, string name, object value)
    {
        Debug.Log(value);
        switch (type)
        {
            case AnimatorControllerParameterType.Bool:
                anim.SetBool(name, (bool)value);
                break;
            case AnimatorControllerParameterType.Int:
                anim.SetInteger(name, (int)value);
                break;
            case AnimatorControllerParameterType.Float:
                anim.SetFloat(name, (float)value);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            SyncAnimatorData syncAnim = NetworkUtils.GetNetworkData<SyncAnimatorData>(sync["animator_parameters"]);
            foreach (SyncAnimatorParameter parameter in syncAnim.parameters)
            {
                SetAnimatorParameterValue(parameter.type, parameter.name, parameter.value);
            }
        }
        catch (KeyNotFoundException)
        {
            // host hasn't sent anything yet
        }
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        SyncAnimatorTriggerMessage trigger = NetworkUtils.GetNetworkData<SyncAnimatorTriggerMessage>(message.contents);
        if (trigger != null)
        {
            anim.SetTrigger(trigger.name);
        }
    }
}
