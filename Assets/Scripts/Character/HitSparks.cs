using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitSparksEffect
{
    NONE, HIT_SPARKS_1
}

public class HitSparks : MonoBehaviour
{
    Animator anim;
    public HitSparksEffect Effect { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(DestroyAfter(4f));
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("Animation", (int)Effect);
    }

    public void SetAnimation(HitSparksEffect ef)
    {
        Effect = ef;
    }

    IEnumerator DestroyAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
        yield break;
    }
}
