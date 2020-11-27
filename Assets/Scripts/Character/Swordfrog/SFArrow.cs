using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFArrow : MonoBehaviour
{

    Rigidbody2D rb;
    Animator anim;

    public bool Active { get; set; } = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        StartCoroutine(Fade(5f));
    }

    IEnumerator Fade(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
        yield break;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Platform")
        {
            foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.enabled = false;
                }

            StartCoroutine(Fade(.5f));
            rb.velocity = Vector2.zero;
            rb.freezeRotation = true;
            rb.gravityScale = 0;
        }
    }
}
