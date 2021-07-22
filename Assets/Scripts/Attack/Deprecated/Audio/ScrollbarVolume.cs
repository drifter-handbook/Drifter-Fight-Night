using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ScrollbarVolume : MonoBehaviour
{

    public Scrollbar scrollbar;
    public AudioMixer mixer;
    public string channel;
    // Start is called before the first frame update
    void Start()
    {
        scrollbar.onValueChanged.AddListener((float val) => ScrollbarCallback(val));
    }

    void ScrollbarCallback(float value)
    {
        mixer.SetFloat(channel, 100 * value - 80);
    }
}
