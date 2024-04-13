using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushbox : MonoBehaviour{
	public bool hasCollision = true;
   	void OnCollisionStay2D(Collision2D col) {
   		if(hasCollision) gameObject.transform.parent.GetComponent<Rigidbody2D>().AddForce(new Vector2(
			-1 * Mathf.Sign(col.gameObject.transform.position.x-gameObject.transform.position.x) * Mathf.Clamp(1/Mathf.Abs(col.gameObject.transform.position.x-gameObject.transform.position.x),.5f,10f), 0), ForceMode2D.Impulse);
	}
}
