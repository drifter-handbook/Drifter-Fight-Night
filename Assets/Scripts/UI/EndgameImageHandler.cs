using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndgameImageHandler : MonoBehaviour
{
    
    public Sprite[] sprites;
    public GameObject[] winnerSetups;

    public GameObject rightPanel;
    public GameObject sillyImagePrefab;

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
                setup.transform.GetChild(0).GetComponent<Text>().color = color; //sets player Color on shadow text
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
        miniIcon.transform.GetChild(0).GetComponent<Image>().color = color; //sets player Color on circle
        switch (type)
        {
            case DrifterType.Nero: miniIcon.GetComponent<Image>().sprite = sprites[0]; break;
            case DrifterType.Orro: miniIcon.GetComponent<Image>().sprite = sprites[1]; break;
            case DrifterType.Bojo: miniIcon.GetComponent<Image>().sprite = sprites[2]; break;
            case DrifterType.Ryyke: miniIcon.GetComponent<Image>().sprite = sprites[3]; break;
            case DrifterType.Swordfrog: miniIcon.GetComponent<Image>().sprite = sprites[4]; break;
            case DrifterType.Megurin: miniIcon.GetComponent<Image>().sprite = sprites[5]; break;
            case DrifterType.Spacejam: miniIcon.GetComponent<Image>().sprite = sprites[6]; break;
            case DrifterType.Lady_Parhelion:
            default: miniIcon.GetComponent<Image>().sprite = sprites[7]; break;                        
        }
    }

    public void Exit()
    {
        if (GameController.Instance.GetComponent<NetworkClient>() != null)
        {
            SceneManager.LoadScene("MenuScene");
        }

        if (GameController.Instance.GetComponent<NetworkHost>() != null)
        {
            SceneManager.LoadScene("MenuScene");
        }
    }
}
