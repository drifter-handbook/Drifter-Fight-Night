using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToTarget : MonoBehaviour
{
    public GameObject victim;

    void Update()
    {
        if(!GameController.Instance.IsHost)return;
        if(victim != null) transform.position  = victim.transform.position;
    }

    public void playStateIfHasVictim(string state)
    {
        if(!GameController.Instance.IsHost)return;
        if(victim != null)GetComponent<SyncAnimatorStateHost>().SetState(state);
    }

    public void clearVictim()
    {
        if(!GameController.Instance.IsHost)return;
        victim = null;
    }
}
