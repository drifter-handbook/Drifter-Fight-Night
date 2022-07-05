using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Infector : MonoBehaviour
{

    public LucilleMasterHit Lucille;


    public void infectVictim()
    {
        foreach(PuppetGrabHitboxCollision infector in GetComponentsInChildren<PuppetGrabHitboxCollision>(true))
        {
            if(infector.victim != null)
            {
                Lucille.infect(infector.victim);
                return;
            }
        }
    }
}