using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSound : MonoBehaviour
{
    [SerializeField] public AudioClip[] Clips;


    public void PlayAudio()
    {
        AudioSource source = GameController.Instance.GetComponent<AudioSource>();
        source.clip = Clips[UnityEngine.Random.Range(0, Clips.Length)];
        source.PlayOneShot(source.clip);
    }

    public void PlayAudio(int index)
    {
        AudioSource source = GameController.Instance.GetComponent<AudioSource>();
        source.clip = Clips[index];
        source.PlayOneShot(source.clip);
    }
}
