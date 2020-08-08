using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* Holds info for one character about their performance during a match. 
* Only tracks for one character, ie. create a new one for a new player!!
*/
public class Statistics : MonoBehaviour
{
    List<Drifter> kills = new List<Drifter>();

    int seppukuCount;
    float damageDone;
    float damageTaken;
    float damageBlocked;
    Drifter lastHit;

    public void DieFool()
    {
        
    }
}