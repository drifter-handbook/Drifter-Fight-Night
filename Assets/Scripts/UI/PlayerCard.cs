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
    public int hasChargeCounter = 0;

    public GameObject TopObject;
    public GameObject BottomObject;

    public Sprite[] portraits_no_Charge;
    public Sprite[] portraits_with_Charge;
    public Sprite[] portraits_one_Charge;
    public Sprite[] Charge_Ticks;

    public SpriteRenderer chargeBar;

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

        for(int i = 1; i< MAX_STOCKS; i++)
        {
            GameObject newStock = Instantiate(stock, new Vector3(0,0), Quaternion.identity);
            newStock.transform.SetParent(stockHolder.transform, false);
            newStock.transform.localScale = new Vector3(1, 1, 1);
            newStock.GetComponent<Image>().sprite = stockImage;
            currStocks++;
        }
    }

    public void addStatusBar(PlayerStatusEffect statusEffect,int icon, float duration, PlayerStatus status)
    {

        GameObject newBar = Instantiate(bar, new Vector3(0,0), Quaternion.identity);
        newBar.transform.SetParent(statusHolder.transform, false);
        newBar.transform.localScale = new Vector3(100, 100, 1);
        newBar.GetComponent<StatusBar>().status = status;
        newBar.GetComponent<StatusBar>().initialize(statusEffect,icon,duration);
        //currentStatusCount++;

    }

    public void SetColor(int color)
    {
        mycolor = color;

        switch(hasChargeCounter)
        {
            case(1):
                gameObject.GetComponent<SpriteRenderer>().sprite = portraits_one_Charge[color];
                break;
            case(3):
                gameObject.GetComponent<SpriteRenderer>().sprite = portraits_with_Charge[color];
                break;
            case 0:
            default:
                gameObject.GetComponent<SpriteRenderer>().sprite = portraits_no_Charge[color];
                break;
        }

    }

    public void SetCharge(int charge)
    {

        if(hasChargeCounter <=0)return;

        chargeBar.sprite = Charge_Ticks[charge];

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

    public void setImages(Sprite face, Sprite stock)
    {
        this.face.GetComponent<SpriteRenderer>().sprite = face;
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
            StartCoroutine(TopShake.Shake(.3f,(sentPercent - previousPercent)/120f));
            StartCoroutine(BottomShake.Shake(.3f,(sentPercent - previousPercent)/120f));
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
