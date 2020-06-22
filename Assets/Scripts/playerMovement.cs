using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public PlayerInputData input { get; set; } = new PlayerInputData();
    public int numberOfJumps = 2;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (input.MoveX > 0)
        {
            rb.velocity = 5f * Vector2.left;
        }
        if (input.MoveX < 0)
        {
            rb.velocity = 5f * Vector2.right;
        }
        if (input.MoveY > 0)
        {
            if (numberOfJumps > 0)
            {
                numberOfJumps--;
                rb.velocity = new Vector2(rb.velocity.x, 10);
            }
        }
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector3.down, 3.2f + 0.25f);
    }
}
