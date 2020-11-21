using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroSpear : MonoBehaviour
{
    public Vector2 velocity;

    Rigidbody2D rb;
    Animator anim;

    public bool Active { get; set; } = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            rb.velocity = velocity;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
        yield break;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Platform")
        {
            if (Active)
            {
                StartCoroutine(Fade());
            }
            Active = false;
            anim.SetBool("Active", Active);
        }
    }
}
