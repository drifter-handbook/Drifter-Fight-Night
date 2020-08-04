using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    public GameObject face;
    public Sprite stockImage;
    public Text percent;
    public int currStocks = 0;

    const int MAX_STOCKS = 4;


    public GameObject stockHolder;

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
        Debug.Log("Running2");
        for (int i = 0; i<num; i++)
        {
            Debug.Log("Runnin3");
            Destroy(stockHolder.transform.GetChild(0).gameObject);
        }

    }


    public void setImages(Sprite face, Sprite stock)
    {
        this.face.GetComponent<Image>().sprite = face;
        this.stockImage = stock;
    }

    internal void removeToStock(int stocks)
    {
        if (currStocks > stocks)
        {
            Debug.Log("Running");
            removeStock();
        }
    }

    public void setPercent(float sentPercent)
    {
        this.percent.text = (int)sentPercent+"%";
    }


}
