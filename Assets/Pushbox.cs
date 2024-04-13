using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushbox : MonoBehaviour{
   void OnTriggerStay2D(Collider2D col) {
		gameObject.transform.parent.GetComponent<Rigidbody2D>().AddForce(new Vector2(
			-1 * Mathf.Sign(col.gameObject.transform.position.x-gameObject.transform.position.x) * Mathf.Clamp(1/Mathf.Abs(col.gameObject.transform.position.x-gameObject.transform.position.x),.5f,15f), 0), ForceMode2D.Impulse);
	}
}
