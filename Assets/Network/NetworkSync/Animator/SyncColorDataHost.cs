using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncColorDataHost : MonoBehaviour, ISyncHost
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
        sync["colorInfo"] = 
            new SyncInt
            {
                integerValue = drifter.GetColor()
            };
    }

    public void setColor(int colorCode)
    {
        if (GameController.Instance.IsHost && GameController.Instance.IsOnline)
        {
            sync.SendNetworkMessage(new SyncInt() { integerValue = colorCode});
        }
    }
}

public class SyncInt : INetworkData
{
    public int integerValue;
    public string Type { get; set; }
}