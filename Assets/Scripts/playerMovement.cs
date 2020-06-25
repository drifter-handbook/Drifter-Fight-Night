using System.Collections;
using System.Collections.Generic;
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
        if (input.MoveX < 0)
        {
            rb.velocity = new Vector2(-25f, rb.velocity.y);
        }
        else if (input.MoveX > 0)
        {
            rb.velocity = new Vector2(25f, rb.velocity.y);
        }
        else
        {
            float x = Mathf.MoveTowards(rb.velocity.x, 0f, 40f * Time.deltaTime);
            rb.velocity = new Vector2(x, rb.velocity.y);
        }
        // ground behavior
        if (IsGrounded())
        {
            if (rb.velocity.y < 55f * 0.1f)
            {
                numberOfJumps = 1;
            }
        }
        // jump
        if (input.MoveY > 0 && rb.velocity.y < 55f * 0.2f)
        {
            if (IsGrounded())
            {
                numberOfJumps = 2;
            }
            if (numberOfJumps > 0)
            {
                numberOfJumps--;
                rb.velocity = new Vector2(rb.velocity.x, 55f);
            }
        }
    }

    RaycastHit2D[] hits = new RaycastHit2D[10];
    public bool IsGrounded()
    {
        int count = Physics2D.RaycastNonAlloc(transform.position + 3.9f * Vector3.down, Vector3.down, hits, 0.2f);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider.gameObject.tag == "Ground")
            {
                return true;
            }
        }
        return false;
    }
}
