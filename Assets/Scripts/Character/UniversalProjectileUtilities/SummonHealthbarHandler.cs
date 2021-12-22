using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SummonHealthbarHandler : MonoBehaviour
{
    // Start is called before the first frame update
    private int _facing = 1;
	public int facing
    {
        get { return _facing;}
        set {
            _facing = value;
            rectTransform.transform.localScale = new Vector2(value * .625f,.625f);
        }
    }
    float maxBarFadeTime = 1.5f;
    float barFadeTime;
    public Image bar;
    SyncAnimatorStateHost anim;


    RectTransform rectTransform;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        anim = GetComponent<SyncAnimatorStateHost>();
        barFadeTime = maxBarFadeTime;
    }

    //TODO Make this not awful, and make it work for multiplayer
    // void Start()
    // {
    //     foreach (Image barChild in GetComponentsInChildren<Image>(true))
    //     {
    //         //Currently doesnt work for clients, and clients would not recieve color data in time
    //         barChild.color = CharacterMenu.ColorFromEnum[(PlayerColor)rectTransform.transform.parent.gameObject.GetComponent<SyncProjectileColorDataHost>().color];
    //     }
    //     GetComponent<Image>().color = UnityEngine.Color.white;
    // }

    // Update is called once per frame
    void Update()
    {
        if(barFadeTime < maxBarFadeTime)
        {
        	barFadeTime += Time.deltaTime;
        	if(barFadeTime >=maxBarFadeTime)
        		anim.SetState("Hide");
        }
    }

    public void updateHealthbar(float percentage)
    {
    	barFadeTime = 0f;
    	bar.fillAmount = percentage;
    	anim.SetState(percentage > 0 ? "Show":"Hide");

    }
}
