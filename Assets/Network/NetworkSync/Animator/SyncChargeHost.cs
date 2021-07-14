using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncChargeHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;

    Drifter drifter;

    // Start is called before the first frame update
    void Awake()
    {
        sync = GetComponent<NetworkSync>();
        drifter = GetComponent<Drifter>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameController.Instance.IsOnline)return;
        sync["chargeInfo"] = 
            new SyncInt
            {
                integerValue = drifter.GetCharge()
            };
    }

    public void setCharge(int charge)
    {
        if (GameController.Instance.IsHost && GameController.Instance.IsOnline)
        {
            sync.SendNetworkMessage(new SyncInt() { integerValue = drifter.GetCharge()});
        }
    }
}