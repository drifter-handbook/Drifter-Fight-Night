using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shows players and selected character [View]
public class CharacterCard : MonoBehaviour
{
    public SpriteRenderer stage;
    public SpriteRenderer flourish;

    Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void SetCharacter(DrifterType drifter)
    {
        anim.Play(drifter.ToString().Replace("_", " "));
    }

    public void SetStage(Sprite sprite)
    {
        stage.sprite = sprite;
    }

    public void setColor(int color)
    {

    }

    // //try to add player, return false if over max
    // public static GameObject CreatePlayerCard(Color color)
    // {
    //     // TODO: Show specific character based on selection
    //     GameObject card = Instantiate(GameController.Instance.characterCardPrefab);
    //     card.transform.GetChild(0).GetComponent<Image>().color = color;
    //     Animate(card.transform);
    //     return card;
    // }

    // public static void Animate(Transform card)
    // {
    //     card.localScale = new Vector3(1, 1, 1);
    //     card.localPosition = new Vector3(card.transform.localPosition.x, card.transform.localPosition.y, 0);

    // }

    // public static void SetCharacter(Transform card, Sprite sprite, string name)
    // {
    //     card.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = sprite;
    //     card.transform.GetChild(1).GetComponent<Text>().text = name;
    // }

    // public static Button EnableKickPlayers(Transform card, bool enable)
    // {
    //     Button kick = card.transform.GetChild(2).GetComponent<Button>();
    //     card.transform.GetChild(2).gameObject.SetActive(enable);

    //     if (enable)
    //     {
    //         return kick;
    //     }

    //     return null;
    // }


}
