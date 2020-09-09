using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndgameImageHandler : MonoBehaviour
{
    Image picture;
    public Sprite[] sprites;

    void Awake()
    {
    	picture = GetComponent<Image>();
    }

    public void setImage(string winner){
        int winnerIndex = -1;
        if (winner != null)
        {
            winnerIndex = int.Parse(winner.Split('|')[1]);
            gameObject.GetComponent<MultiSound>().PlayAudio(winnerIndex);
            winner = winner.Split('|')[0];
        }
        
        UnityEngine.Debug.Log(winner);
        switch(winner){
            case "Nero":
                picture.sprite = sprites[0];
                break;
            case "Orro":
                picture.sprite = sprites[1];
                break;
            case "Bojo":
                picture.sprite = sprites[2];
                break;
            case "Ryyke":
                picture.sprite = sprites[3];
                break;
            case "Swordfrog":
                picture.sprite = sprites[4];
                break;
            case "Megurin":
                picture.sprite = sprites[5];
                break;
            case "Spacejam":
                picture.sprite = sprites[6];
                break;
            case "Lady Parhelion":
                picture.sprite = sprites[7];
                break;

            default:
                UnityEngine.Debug.Log("NO IMAGE FOR: " + winner);
                break;                            
        }
    }

}
