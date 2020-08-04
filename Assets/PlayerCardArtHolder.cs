using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardArtHolder : MonoBehaviour
{
    public Sprite[] faces = new Sprite[8];
    public Sprite[] stocks = new Sprite[8];
    public GameObject summaryCardPrefab;
    public GameObject stockPrefab;

    private Drifter[] drifters;
    private PlayerCard[] playerCards;

    void Awake()
    {
        drifters = FindObjectsOfType<Drifter>();

        playerCards = new PlayerCard[drifters.Length];

        int i = 0; //i know i know i just like foreach ok
        Debug.Log(drifters.Length);
        foreach(Drifter drifter in drifters)
        {
           GameObject newCard = Instantiate(summaryCardPrefab, transform.position, transform.rotation);
            newCard.transform.parent = gameObject.transform;
            newCard.transform.localScale = new Vector3(1, 1, 1);
            playerCards[i] = newCard.GetComponent<PlayerCard>();
            int imageIndex = getDrifterTypeIndex(drifter.drifterData);
            playerCards[i].setImages(faces[imageIndex], stocks[imageIndex]);
            playerCards[i].addStocks(stockPrefab,3);
            i++;
        }
    }

    private void Update()
    {
        for(int i = 0; i< drifters.Length; i++)
        {
            playerCards[i].setPercent(drifters[i].DamageTaken);
        }
    }


    private int getDrifterTypeIndex(DrifterData data)
    {
        switch (data.ReadableName)
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
