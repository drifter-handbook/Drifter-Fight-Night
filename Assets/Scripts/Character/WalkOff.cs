using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkOff : MonoBehaviour
{

	public Rigidbody2D rb;
	bool preventWalkOff = false;
	bool touchingGround = false;
    // Start is called before the first frame update

    void FixedUpdate()
    {
    	if(preventWalkOff && !touchingGround)
    		rb.velocity = new Vector2(0,rb.velocity.y);
    	touchingGround = false;
    }

    void OnTriggerStay2D(Collider2D collider)
    {
    	if(collider.gameObject.tag == "Ground" || collider.gameObject.tag == "Platform")
    		touchingGround = true;
    }

    public void togglePreventWalkoff()
    {
    	preventWalkOff = touchingGround;
    }

    public void setPreventWalkoff(bool toggle)
    {
    	preventWalkOff = toggle;
    }
}
