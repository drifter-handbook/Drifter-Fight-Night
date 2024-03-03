using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkOff : MonoBehaviour
{

	public Rigidbody2D rb;
	bool preventWalkOff = false;
	bool touchingGround = false;
	// Start is called before the first frame update

	public void UpdateFrame() {
		if(preventWalkOff && !touchingGround){
			rb.velocity = new Vector2(rb.velocity.x != 0 ? Mathf.Sign(rb.velocity.x) * -2 : 0,rb.velocity.y);
			touchingGround = false;
		}
		
	}

	void OnTriggerStay2D(Collider2D collider) {
		if(collider.gameObject.tag == "Ground" || collider.gameObject.tag == "Platform")
			touchingGround = true;
	}

	void OnTriggerExit2D(Collider2D collider) {
		if(collider.gameObject.tag == "Ground" || collider.gameObject.tag == "Platform")
		{
			touchingGround = false;
			if(preventWalkOff)
				rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * -2,rb.velocity.y);
		}

	}

	public bool IsTouchingGround(){
		return touchingGround;
	}

	public void togglePreventWalkoff() {
		preventWalkOff = touchingGround;
	}

	public void setPreventWalkoff(bool toggle) {
		preventWalkOff = toggle;
	}
}
