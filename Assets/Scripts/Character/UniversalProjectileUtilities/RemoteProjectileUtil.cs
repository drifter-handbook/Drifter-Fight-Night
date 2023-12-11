using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RemoteProjectileUtil : MonoBehaviour
{
    public MasterHit hit;
    public int PorjectileIndex = 0;

    public void TriggerRemoteSpawn()
    {
        hit.TriggerRemoteSpawn(PorjectileIndex);
    }

}
