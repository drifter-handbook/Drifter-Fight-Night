using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncProjectileColorDataHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;

    SpriteRenderer sprite;

    int color = 0;

    // Start is called before the first frame update
    void Awake()
    {
        sync = GetComponent<NetworkSync>();
        sprite = GetComponent<SpriteRenderer>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameController.Instance.IsOnline)return;
        sync["colorInfo"] = 
            new SyncInt
            {
                integerValue = color
            };
    }

    public void setColor(int colorCode)
    {
        if (GameController.Instance.IsHost && GameController.Instance.IsOnline)
        {
            color = colorCode;
            sprite.material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)colorCode]);
            sync.SendNetworkMessage(new SyncInt() { integerValue = colorCode});
        }
    }
}