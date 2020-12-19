using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
