using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour , INetworkInit
{

    public Sprite[] statusIcons;

    public SpriteRenderer Icon;

    public PlayerStatus status;

    public SpriteMask mask;

    float duration;
    PlayerStatusEffect ef;

    public void OnNetworkInit()
    {
        NetworkUtils.RegisterChildObject("StatusBarMask", mask.gameObject);
    }

    void Update()
    {
        if(duration == 0) Destroy(gameObject);
        mask.transform.localPosition = new Vector2((.67f * status.remainingDuration(ef)/(duration * 10f)) + .05f,0);

        if(status.remainingDuration(ef) <= 0) Destroy(gameObject);

    } 

    public void initialize(PlayerStatusEffect statusEffect,int index, float duration)
    {

        ef = statusEffect;
        Icon.sprite = statusIcons[index];
        this.duration = duration;

    }
}