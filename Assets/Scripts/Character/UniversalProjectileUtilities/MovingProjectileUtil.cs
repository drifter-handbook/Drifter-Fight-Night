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
        if(!GameController.Instance.IsHost)return;
        rb = GetComponent<Rigidbody2D>();
    }

    public void setYVelocity(float y)
    {
        if(!GameController.Instance.IsHost)return;
        rb.velocity = new Vector2(rb.velocity.x,y);
    }

    public void setXVelocity(float x)
    {
        if(!GameController.Instance.IsHost)return;

        UnityEngine.Debug.Log(Mathf.Sign(rb.velocity.x) * x );
        rb.velocity = new Vector2( Mathf.Sign(rb.velocity.x) * x ,rb.velocity.y);
    }

    public void setGravity(float gravity)
    {
    	if(!GameController.Instance.IsHost)return;

    	rb.gravityScale = gravity;
    	
    }

    public void freeze()
    {
        if(!GameController.Instance.IsHost)return;

        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        if(!GameController.Instance.IsHost)return;
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
