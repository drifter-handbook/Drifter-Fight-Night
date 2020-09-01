using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleSound : MonoBehaviour
{
    [SerializeField] public AudioClip Clip;


    public void PlayAudio()
    {
        AudioSource source = GameController.Instance.GetComponent<AudioSource>();
        source.clip = Clip;
        source.PlayOneShot(source.clip);
    }
}
