using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardArtHolder : MonoBehaviour
{
    //public Sprite[] faces;// = new Sprite[8];
    public Sprite[] stocks;// = new Sprite[8];
    public GameObject summaryCardPrefab;
    public GameObject miniSummaryCardPrefab;
    public GameObject stockPrefab;
    public TrainingUIManager trainingUI;

    public Drifter[] drifters;
    private PlayerCard[] playerCards;

    public GameObject mainCamera;

    void FixedUpdate() {
        if (drifters == null || drifters.Length == 0)
        {

            if(!GameController.Instance.IsTraining)
                trainingUI.gameObject.SetActive(false);

            drifters = FindObjectsOfType<Drifter>();

            playerCards = new PlayerCard[drifters.Length];

             //i know i know i just like foreach ok
            for(int i = 0; i < drifters.Length; i++)
            {

                GameObject newCard;
     
                newCard = Instantiate(summaryCardPrefab, transform.position, transform.rotation);
                    
                newCard.transform.SetParent(gameObject.transform , false);
                newCard.transform.localScale = new Vector3(1, 1, 1);
                playerCards[i] = newCard.GetComponent<PlayerCard>();

                
                int imageIndex = (int)drifters[i].drifterType;

                //Colors
                playerCards[i].SetColor(drifters[i].myColor);
                drifters[i].SetColor(drifters[i].myColor);

                drifters[i].status.card = playerCards[i];
                
                playerCards[i].drifterIndex = imageIndex;

                playerCards[i].setImages(stocks[imageIndex]);
                playerCards[i].addStocks(stockPrefab, 4);

                //Add dummy and player to training UI
                if(GameController.Instance.IsTraining){
                    drifters[i].GetComponent<PlayerHurtboxHandler>().trainingUI = trainingUI;
                    if(drifters[i].peerID == 8) trainingUI.DummyHandler.Dummy = drifters[i];
                    else
                        trainingUI.DummyHandler.Player= drifters[i];
                }
                

            }
            //if(mainCamera == null) GameObject.FindGameObjectWithTag("MainCamera");
            mainCamera.GetComponent<ScreenShake>().drifters = drifters;
        }

        //For each drifter, update their card
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
}
