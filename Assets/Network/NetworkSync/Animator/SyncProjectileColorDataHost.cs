using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncProjectileColorDataHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;

    SpriteRenderer sprite;

    // Start is called before the first frame update
    void Awake()
    {
        sync = GetComponent<NetworkSync>();
        sprite = GetComponent<SpriteRenderer>();
       
    }

    // Update is called once per frame
    // void Update()
    // {
    //     sync["colorInfo"] = 
    //         new SyncInt
    //         {
    //             integerValue = drifter.GetColor()
    //         };
    // }

    public void setColor(int colorCode)
    {
        if (GameController.Instance.IsHost)
        {
            sprite.material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)colorCode]);
            sync.SendNetworkMessage(new SyncInt() { integerValue = colorCode});
        }
    }
}