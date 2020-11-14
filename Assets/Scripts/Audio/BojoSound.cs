using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BojoSound : MonoBehaviour
{
    [SerializeField] public AudioClip[] Clips;

    AudioSource source;

    public void Start()
    {
        source = GameController.Instance.GetComponent<AudioSource>();
    }
    public void PlayAudio()
    {
        source.clip = Clips[UnityEngine.Random.Range(0, Clips.Length)];
        source.PlayOneShot(source.clip);
    }

    public void PlayAudio(int index)
    {
        if(index <= Clips.Length){
            source.clip = Clips[index];
            source.PlayOneShot(source.clip);
        }
       
    }
}
