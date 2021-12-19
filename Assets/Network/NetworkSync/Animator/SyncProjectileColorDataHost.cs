﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncProjectileColorDataHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;

    SpriteRenderer sprite;

    public int color = 0;

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
        if (GameController.Instance.IsHost)
        {
            color = colorCode;
            sprite.material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)colorCode]);
            if(GameController.Instance.IsOnline)sync.SendNetworkMessage(new SyncInt() { integerValue = colorCode});
        }
    }
}