using UnityEngine;
using UnityEngine.UI;

public class Figurine : MonoBehaviour
{
    public GameObject arrow;
    
    public void TurnArrowOn()
    {
        arrow.transform.parent.gameObject.SetActive(true);
    }

    public void TurnArrowOff()
    {
        arrow.transform.parent.gameObject.SetActive(false);
    }

    public void SetColor(Color color)
    {
        arrow.GetComponent<Image>().color = color;
    }

}