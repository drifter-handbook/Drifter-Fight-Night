using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAnimatorHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;

    List<SyncAnimatorParameter> parameters = new List<SyncAnimatorParameter>();

    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        anim = GetComponent<Animator>();
        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            parameters.Add(new SyncAnimatorParameter() {
                name = parameter.name,
                type = parameter.type,
                value = GetAnimatorParameterValue(parameter.type, parameter.name)
            });
        }
    }

    object GetAnimatorParameterValue(AnimatorControllerParameterType type, string name)
    {
        switch(type)
        {
            case AnimatorControllerParameterType.Bool:
                return anim.GetBool(name);
            case AnimatorControllerParameterType.Int:
                return anim.GetInteger(name);
            case AnimatorControllerParameterType.Float:
                return anim.GetFloat(name);
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (SyncAnimatorParameter parameter in parameters)
        {
            parameter.value = GetAnimatorParameterValue(parameter.type, parameter.name);
        }
        sync["animator_parameters"] = new SyncAnimatorData() { parameters = parameters };
    }

    public void SetTrigger(string name)
    {
        anim.SetTrigger(name);
        if (GameController.Instance.IsHost)
        {
            sync.SendNetworkMessage(new SyncAnimatorTriggerMessage() { name = name });
        }
    }
}

public class SyncAnimatorParameter
{
    public string name;
    public AnimatorControllerParameterType type;
    public object value;
}

public class SyncAnimatorData : INetworkData
{
    public string Type { get; set; }
    public List<SyncAnimatorParameter> parameters;
}

public class SyncAnimatorTriggerMessage : INetworkData
{
    public string Type { get; set; }
    public string name;
}
