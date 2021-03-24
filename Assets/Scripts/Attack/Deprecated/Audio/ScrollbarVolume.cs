using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScrollbarVolume : MonoBehaviour
{

    public Scrollbar scrollbar;
    public GameController.VolumeType volumeType;
    // Start is called before the first frame update
    void Start()
    {
        scrollbar.onValueChanged.AddListener((float val) => ScrollbarCallback(val));
    }

    void ScrollbarCallback(float value)
    {
        if(volumeType == GameController.VolumeType.MUSIC)
        {
            GameController.Instance.volume[(int)volumeType] = value;
        }
        else if(volumeType == GameController.VolumeType.SFX)
        {
            GameController.Instance.UpdateSFXVolume(value);
        }
    }
}
