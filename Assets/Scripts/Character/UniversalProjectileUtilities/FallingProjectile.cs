using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingProjectile : MonoBehaviour
{
    public float speed = -55;
    // Start is called before the first frame update
    Rigidbody2D rb;

    void Start()
    {
        if(!GameController.Instance.IsHost)return;
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(0,speed);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!GameController.Instance.IsHost)return;
        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Platform")
        {
            rb.velocity = Vector2.zero;
            GetComponent<Animator>().Play("Delete");
        }
    }
}
