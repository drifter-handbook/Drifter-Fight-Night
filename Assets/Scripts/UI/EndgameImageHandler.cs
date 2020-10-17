using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndgameImageHandler : MonoBehaviour
{
    
    public Sprite[] sprites;
    public GameObject[] winnerSetups;

    public GameObject rightPanel;
    public GameObject sillyImagePrefab;

    /*
     *  int winnerIndex = -1;
        if (type != DrifterType.None)
        {
            winnerIndex = int.Parse(winner.Split('|')[1]);
            
            winner = winner.Split('|')[0];
        }
        */

    public void playWinnerAudio(int winnerIndex)
    {
        gameObject.GetComponent<MultiSound>().PlayAudio(winnerIndex);
    }

    public void setWinnerPic(DrifterType type, Color color)
    {

        foreach(GameObject setup in winnerSetups)
        {
            if (setup.name.Contains(type.ToString()))
            {
                setup.SetActive(true);
                setup.transform.GetChild(0).GetComponent<Image>().color = color; //sets player Color on shadow text
            } else
            {
                setup.SetActive(false);
            }
        }
    }

    public void setSillyImage(DrifterType type, Color color)
    {
        GameObject miniIcon = Instantiate(sillyImagePrefab);
        Transform parent = rightPanel.transform;
        miniIcon.transform.SetParent(parent, false);
        Sprite picture = miniIcon.GetComponent<Image>().sprite;
        miniIcon.transform.GetChild(0).GetComponent<Image>().color = color; //sets player Color on circle
        switch (type)
        {
            case DrifterType.Nero: picture = sprites[0]; break;
            case DrifterType.Orro: picture = sprites[1]; break;
            case DrifterType.Bojo: picture = sprites[2]; break;
            case DrifterType.Ryyke: picture = sprites[3]; break;
            case DrifterType.Swordfrog: picture = sprites[4]; break;
            case DrifterType.Megurin: picture = sprites[5]; break;
            case DrifterType.Spacejam: picture = sprites[6]; break;
            case DrifterType.Lady_Parhelion:
            default:  picture = sprites[7]; break;                        
        }
    }

}
