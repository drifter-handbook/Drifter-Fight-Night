using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jukebox : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] soundtrack;
    [Range(0, 1)]
    public float volume = 0.5f;

    private Random rand;
    private int selectedSong = -1;
    private float lastVolume = 0;


    void Initialize(int seed)
    {
        Random.InitState(seed);
    }

    void InitializeWithSong(int index)
    {
        selectedSong = index;
    }

    private void Update()
    {
        if (lastVolume != volume)
        {
            audioSource.volume = volume;
            lastVolume = volume;
        }
        if(GameController.Instance.volume != -1)
        {
            volume = GameController.Instance.volume;
            audioSource.volume = volume;
            lastVolume = volume;
        }
    }

    void Start()
    {
        //pick your sound!
        if(selectedSong < 0)
        {
            selectedSong = (int)(Random.value*(soundtrack.Length-1));
        }
        audioSource.clip = soundtrack[selectedSong];
        audioSource.Play();
    }

}
