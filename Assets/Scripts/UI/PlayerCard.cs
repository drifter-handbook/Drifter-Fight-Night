using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    public GameObject face;
    public Sprite stockImage;
    public int currStocks = 0;
    public int drifterIndex;

    public GameObject TopObject;
    public GameObject BottomObject;

    // public Sprite[] portraits_no_Charge;
    // public Sprite[] portraits_with_Charge;
    // public Sprite[] portraits_one_Charge;
    public Sprite[] levels;

    public RectTransform chargeBar;
    public RectTransform chargeMask;

    public Image chargeLevels;

    public Image ribbons;

    public GameObject bar;

    public GameObject stockHolder;
    public GameObject statusHolder;

    Text TopText;
    Text BottomText;

    GameObjectShake TopShake;
    GameObjectShake BottomShake;

    int mycolor; 
    float previousPercent = 0f;

    const int MAX_STOCKS = 4;


    void Awake()
    {

        TopShake = TopObject.GetComponent<GameObjectShake>();
        TopText = TopObject.GetComponent<Text>();

        BottomShake = BottomObject.GetComponent<GameObjectShake>();
        BottomText = BottomObject.GetComponent<Text>();

    }

    public void addStock(GameObject stock)
    {
        addStocks(stock, 1);
    }
    public void addStocks(GameObject stock, int num)
    {
        if (currStocks + num > MAX_STOCKS)
        {
            num = MAX_STOCKS - currStocks;
        }

        for(int i = 0; i< MAX_STOCKS; i++)
        {
            GameObject newStock = Instantiate(stock, new Vector3(0,0), Quaternion.identity);
            newStock.transform.SetParent(stockHolder.transform, false);
            newStock.transform.localScale = new Vector3(1, 1, 1);
            newStock.GetComponent<Image>().sprite = stockImage;
            currStocks++;
        }
    }

    public GameObject addStatusBar(PlayerStatusEffect statusEffect,int icon, int duration, PlayerStatus status)
    {

        GameObject newBar = Instantiate(bar, new Vector3(0,0), Quaternion.identity);
        newBar.transform.SetParent(statusHolder.transform, false);
        newBar.transform.localScale = new Vector3(100, 100, 1);
        newBar.GetComponent<StatusBar>().status = status;
        newBar.GetComponent<StatusBar>().initialize(statusEffect,icon,duration);
        //currentStatusCount++;
        return newBar;

    }

    public void SetColor(int color)
    {
        mycolor = color;

        ribbons.color = CharacterMenu.ColorFromEnum[(PlayerColor)color];

    }

    public void SetCharge(float charge)
    {
        chargeLevels.sprite = levels[(int)charge];

        int loc = (int)(-(charge/5f) * 65f);

        chargeBar.anchoredPosition  =  new Vector2(-1 *loc,4);
        chargeMask.anchoredPosition  = new Vector2(loc,-4);

    }

    public void removeStock()
    {
        removeStocks(1);
    }

    public void removeStocks(int num)
    {
        if(currStocks <= 0)
        {
            return;
        }
        currStocks -= num;

        if (stockHolder.transform.childCount < num)
        {
            num = stockHolder.transform.childCount;
        }
        for (int i = 0; i<num; i++)
        {
            Destroy(stockHolder.transform.GetChild(0).gameObject);
        }

    }

    public void setImages(Sprite stock)
    {
        //this.face.GetComponent<Image>().sprite = face;
        this.stockImage = stock;
    }

    internal void removeToStock(int stocks)
    {
        if (currStocks > stocks)
        {
            removeStock();
        }
    }

    public void setPercent(float sentPercent)
    {
        if(previousPercent  < sentPercent)
        {
            TopShake.Shake(18,(sentPercent - previousPercent)/2f);
            BottomShake.Shake(18,(sentPercent - previousPercent)/2f);
        }
        previousPercent = sentPercent;

        float greenVal = Mathf.Max((120f - sentPercent)/120f,0);
        float blueVal = Mathf.Max((50f - sentPercent)/50f,0);
        float redVal = Mathf.Max((500f - sentPercent)/500f,.8f);

        this.BottomText.text = sentPercent.ToString("0.0")+"%";

        this.TopText.color = new Color(redVal,greenVal,blueVal,1);
        this.TopText.text = sentPercent.ToString("0.0")+"%";
    }

}
