using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour 
{

    public Sprite[] statusIcons;

    public SpriteRenderer Icon;

    public PlayerStatus status;

    public Image bar;

    float duration;
    PlayerStatusEffect ef;

    public void UpdateFrame() {
        if(status == null) Destroy(gameObject);
        if(duration < status.remainingDuration(ef)) duration = status.remainingDuration(ef);

        if(duration==0) bar.fillAmount = 1f;
        else bar.fillAmount = status.remainingDuration(ef)/duration;

    } 

    public void initialize(PlayerStatusEffect statusEffect,int index, int duration) {
        if(duration == 0) Destroy(gameObject);
        ef = statusEffect;
        Icon.sprite = statusIcons[index];
        this.duration = duration;

    }
}