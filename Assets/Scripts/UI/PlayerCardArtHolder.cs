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
                
                playerCards[i].drifterIndex = imageIndex;
                if(imageIndex == 5 || imageIndex == 1){
                   playerCards[i].charge.SetActive(true);
                }
                else if(imageIndex == 6){
                    playerCards[i].MegurinElements.SetActive(true);
                    playerCards[i].MegurinElements.GetComponent<MegurinGauges>().megurin = drifter.GetComponentInChildren<MegurinMasterHit>();
                }
                else
                {
                    playerCards[i].charge.SetActive(false);
                    playerCards[i].MegurinElements.SetActive(false);
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
            if((playerCards[i].drifterIndex == 5 || playerCards[i].drifterIndex == 1) && !playerCards[i].charge.activeSelf){
                   playerCards[i].charge.SetActive(true);
            }
            else if(playerCards[i].drifterIndex == 6 && !playerCards[i].MegurinElements.activeSelf){
                    playerCards[i].MegurinElements.SetActive(true);
            }
            
            if (drifters[i] != null)
            {
                playerCards[i].removeToStock(drifters[i].Stocks);
            }
            else
            {
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
            default: return 8;
        }
    }

}
