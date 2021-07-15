using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardArtHolder : MonoBehaviour
{
    public Sprite[] faces;// = new Sprite[8];
    public Sprite[] stocks;// = new Sprite[8];
    public GameObject summaryCardPrefab;
    public GameObject miniSummaryCardPrefab;
    public GameObject stockPrefab;
    public TrainingUIManager trainingUI;

    public Drifter[] drifters;
    private PlayerCard[] playerCards;

    public GameObject mainCamera;

    // void Awake()
    // {
    //     mainCamera  = GameObject.FindGameObjectWithTag("MainCamera");
    // }

    private void Update()
    {
        if (drifters == null || drifters.Length == 0)
        {

            if(!GameController.Instance.IsTraining)
                trainingUI.gameObject.SetActive(false);
            // else
            //     trainingUI.gameObject.SetActive(true);


            drifters = FindObjectsOfType<Drifter>();

            playerCards = new PlayerCard[drifters.Length];

             //i know i know i just like foreach ok
            for(int i = drifters.Length -1; i >=0; i--)
            {

                GameObject newCard;
     
                newCard = Instantiate(summaryCardPrefab, transform.position, transform.rotation);
                    
                newCard.transform.SetParent(gameObject.transform , false);
                newCard.transform.localScale = new Vector3(1, 1, 1);
                playerCards[i] = newCard.GetComponent<PlayerCard>();

                
                int imageIndex = getDrifterTypeIndex(drifters[i].GetComponent<NetworkSync>().NetworkType);

                //Colors
                playerCards[i].SetColor(drifters[i].myColor);
                drifters[i].SetColor(drifters[i].myColor);

                drifters[i].status.card = playerCards[i];
                drifters[i].status.trainingUI = trainingUI;
                
                playerCards[i].drifterIndex = imageIndex;

                playerCards[i].setImages(faces[imageIndex], stocks[imageIndex]);
                playerCards[i].addStocks(stockPrefab, 4);

            }
            //if(mainCamera == null) GameObject.FindGameObjectWithTag("MainCamera");
            mainCamera.GetComponent<ScreenShake>().drifters = drifters;
        }

        //For each drifer, update their card
        for (int i = 0; i < drifters.Length; i++)
        {
            playerCards[i].setPercent(drifters[i].DamageTaken);
              
            if (drifters[i] != null)
            {
                playerCards[i].removeToStock(drifters[i].Stocks);
                playerCards[i].SetCharge(drifters[i].superCharge);

                //Prolly remove this
                playerCards[i].SetColor(drifters[i].myColor);
                drifters[i].SetColor(drifters[i].myColor);

                //drifters[i].status.

            }
            else playerCards[i].removeToStock(0);
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
            case ("Lucille"): return 9;
            case ("Mytharius"): return 10;
            case ("Maryam"): return 11;
            case ("Drifter Cannon"): return 12;
            case ("Sandbag"): return 8;
            default: return 8;
        }
    }

}
