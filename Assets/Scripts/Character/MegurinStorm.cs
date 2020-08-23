﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinStorm : MonoBehaviour
{
    public PlayerAttacks attacks;
    public float duration;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Fade(duration));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void multihit(){
        attacks.SetupAttackID(DrifterAttackType.W_Down);
    }


    public IEnumerator Fade(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
        yield break;
    }
}
