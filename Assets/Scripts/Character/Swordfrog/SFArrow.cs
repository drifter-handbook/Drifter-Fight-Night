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
        if(!GameController.Instance.IsHost)return;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(!GameController.Instance.IsHost)return;
        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Platform")
        {
            foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.enabled = false;
                }

            anim.Play("Break");
            rb.velocity = Vector2.zero;
            rb.freezeRotation = true;
            rb.gravityScale = 0;
        }
    }
}
