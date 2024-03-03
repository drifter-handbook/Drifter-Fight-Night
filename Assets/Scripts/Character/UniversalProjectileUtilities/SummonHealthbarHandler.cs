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
    int maxBarFadeTime = 90;
    int barFadeTime = 0;
    public Image bar;
    Animator anim;


    RectTransform rectTransform;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        anim = GetComponent<Animator>();
        barFadeTime = maxBarFadeTime;
    }

    public void setColor(PlayerColor color){
        foreach (Image barChild in GetComponentsInChildren<Image>(true))        
            barChild.color = CharacterMenu.ColorFromEnum[color];
        GetComponent<Image>().color = UnityEngine.Color.white;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(barFadeTime < maxBarFadeTime) {
        	barFadeTime ++;
        	if(barFadeTime >=maxBarFadeTime)
        		anim.Play("Hide");
        }
    }

    public void updateHealthbar(float percentage) {
    	barFadeTime = 0;
    	bar.fillAmount = percentage;
    	anim.Play(percentage > 0 ? "Show":"Hide");

    }
}
