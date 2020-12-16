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
        sync["playerColor"] = 
            new SyncColorState
            {
                color = drifter.GetColor()
            };
    }

    public void SetColor(int colorCode)
    {
        if (GameController.Instance.IsHost)
        {
            sync.SendNetworkMessage(new SyncColorState() { color = colorCode});
        }
    }
}

public class SyncColorState : INetworkData
{
    public int color;
    public string Type { get; set; }
}