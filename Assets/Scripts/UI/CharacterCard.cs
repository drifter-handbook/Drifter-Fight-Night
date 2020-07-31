using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows players and selected character [View]
public class CharacterCard : MonoBehaviour
{

    //try to add player, return false if over max
    public static Transform CreatePlayerCard(Color color)
    {
        // TODO: Show specific character based on selection
        Transform card = Instantiate(GameController.Instance.characterCardPrefab).transform;
        card.localScale = new Vector3(1, 1, 1);
        card.localPosition = new Vector3(card.transform.localPosition.x, card.transform.localPosition.y, 0);
        card.GetChild(0).GetComponent<Image>().color = color;

        return card;
    }

    public static void SetCharacter(Transform card, Sprite sprite, string name) {
        card.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = sprite;
        card.transform.GetChild(1).GetComponent<Text>().text = name;
    }


}
