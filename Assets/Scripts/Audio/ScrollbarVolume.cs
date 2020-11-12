using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScrollbarVolume : MonoBehaviour
{

    public Scrollbar scrollbar;
    // Start is called before the first frame update
    void Start()
    {
        scrollbar.onValueChanged.AddListener((float val) => ScrollbarCallback(val));
    }

    void ScrollbarCallback(float value)
    {
        GameController.Instance.volume = value;
    }
}
