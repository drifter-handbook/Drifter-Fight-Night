using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MovingProjectileUtil : MonoBehaviour
{
    // Start is called before the first frame update

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void setYVelocity(float y)
    {
        rb.velocity = new Vector2(rb.velocity.x,y);
    }

    public void setXVelocity(float x)
    {
        rb.velocity = new Vector2( Mathf.Sign(gameObject.transform.localScale.x) * x ,rb.velocity.y);
    }

    public void setGravity(float gravity)
    {
    	rb.gravityScale = gravity;
    }

    public void freeze()
    {
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        try
        {
            if(collider.gameObject.tag == "Ground") GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Restitution,collider.contacts[0].point,((rb.velocity.x < 0)?1:-1 ) * Vector3.Angle(Vector3.up,collider.contacts[0].normal),Vector3.one);
        }
        catch(NullReferenceException)
        {
            return;
        }
    }
}
