using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LucillePortalCollision : MonoBehaviour
{

    public GameObject drifter;
    public string StateToPlay = "";

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(!GameController.Instance.IsHost)return;

        try{
            if(collider.GetComponent<LucillePortal>().drifter == drifter)
            {
                drifter.GetComponent<Drifter>().PlayAnimation(StateToPlay);
            }
        }
        catch(NullReferenceException)
        {
            return;
        }
    }    
}
