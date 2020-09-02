using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSound : MonoBehaviour
{
    [SerializeField] public AudioClip[] Clips;

    public AudioSource source;


    public void PlayAudio()
    {
        source.clip = Clips[UnityEngine.Random.Range(0, Clips.Length)];
        source.PlayOneShot(source.clip);
    }

    public void PlayAudio(int index)
    {
        source.clip = Clips[index];
        source.PlayOneShot(source.clip);
    }
}
