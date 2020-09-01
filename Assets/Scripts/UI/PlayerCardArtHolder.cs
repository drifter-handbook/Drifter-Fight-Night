using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardArtHolder : MonoBehaviour
{
    public Sprite[] faces = new Sprite[8];
    public Sprite[] stocks = new Sprite[8];
    public GameObject summaryCardPrefab;
    public GameObject miniSummaryCardPrefab;
    public GameObject stockPrefab;

    private Drifter[] drifters;
    private PlayerCard[] playerCards;
    private int[] stockvals;

    NetworkEntityList Entities;

    void Awake()
    {
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
    }

    private void Update()
    {
        if (drifters == null || drifters.Length == 0)
        {
            drifters = FindObjectsOfType<Drifter>();
            stockvals = new int[drifters.Length];

            playerCards = new PlayerCard[drifters.Length];

            int i = 0; //i know i know i just like foreach ok
            foreach (Drifter drifter in drifters)
            {

                GameObject newCard;
                if (drifters.Length > 4)
                {
                    newCard = Instantiate(miniSummaryCardPrefab, transform.position, transform.rotation);
                }
                else
                {
                     newCard = Instantiate(summaryCardPrefab, transform.position, transform.rotation);
                    
                }
                newCard.transform.SetParent(gameObject.transform, false);
                newCard.transform.localScale = new Vector3(1, 1, 1);
                playerCards[i] = newCard.GetComponent<PlayerCard>();

                playerCards[i].SetColor(drifter.myColor);

                int imageIndex = getDrifterTypeIndex(drifter.GetComponent<INetworkSync>().Type);
                if(imageIndex != 5 && imageIndex != 1){
                   Destroy(playerCards[i].charge);
                }
                if(imageIndex !=6){
                    Destroy(playerCards[i].MegurinElements);
                }
                else{
                    playerCards[i].MegurinElements.GetComponent<MegurinGauges>().megurin = drifter.GetComponentInChildren<MegurinMasterHit>();
                }

                playerCards[i].setChargeDrifter(drifter);
                playerCards[i].setImages(faces[imageIndex], stocks[imageIndex]);
                playerCards[i].addStocks(stockPrefab, 3);
                i++;
            }

        }
        for (int i = 0; i < drifters.Length; i++)
        {
            playerCards[i].setPercent(drifters[i].DamageTaken);
            // update stocks
            if (drifters[i] != null)
            {
                stockvals[i] = playerCards[i].currStocks;
                playerCards[i].removeToStock(drifters[i].Stocks);
                
            }
            else
            {
                if (stockvals[i] != playerCards[i].currStocks)
                {
                    if (getNullCount(drifters) > 1)
                    {
                        gameObject.GetComponent<MultiSound>().PlayAudio(i);
                    }
                    else
                        gameObject.GetComponent<SingleSound>().PlayAudio();
                    stockvals[i] = playerCards[i].currStocks;
                }
                playerCards[i].removeToStock(0);
            }
        }
    }

    private int getNullCount(Drifter[] drifters)
    {
        int count = 0;
        for (int i = 0; i < drifters.Length; i++)
            if (drifters[i] == null)
                count++;
        return count;
    }

    private int getDrifterTypeIndex(string name)
    {
        switch (name)
        {
            case ("Bojo"): return 0;
            case ("Swordfrog"): return 1;
            case ("Lady Parhelion"): return 2;
            case ("Spacejam"): return 3;
            case ("Orro"): return 4;
            case ("Ryyke"): return 5;
            case ("Megurin"): return 6;
            case ("Nero"): return 7;
            default: return 7;
        }
    }

}
