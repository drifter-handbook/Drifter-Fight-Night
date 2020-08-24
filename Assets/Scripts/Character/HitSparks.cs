﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSparks : MonoBehaviour
{
    Animator anim;
    public int Effect { get; private set; }
    public AudioSource SoundPlayer;
    public AudioClip[] HitSounds;



    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(DestroyAfter(.3f));
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("Animation", Effect);
    }

    public void SetAnimation(int ef)
    {
        Effect = ef;
        SoundPlayer.clip = HitSounds[ef-1];
        SoundPlayer.Play();
     }

    IEnumerator DestroyAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
        yield break;
    }
}
