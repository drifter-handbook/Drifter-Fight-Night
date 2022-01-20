using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClashProjectile : MonoBehaviour
{
	//-1 indictes infinite priority
	//0 Indicates it will never eat another projectile
	public int priority = 1;

	//The player gameobject that created this projectile
	public GameObject owner;

	//The top level object this detector is attatched to
	public GameObject parent;
	//Dictates if the projectile looses priority when it clashes

	// public GameObject parent;
	Rigidbody2D rb;
    // Start is called before the first frame update
    void Awake()
    {
        rb = parent.GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
    	if(collider.gameObject.layer != 16) return;
    	ClashProjectile other = collider.gameObject.GetComponent<ClashProjectile>();

    	//If the other object doesnt have a clash box or is owned by the same drifter, return
    	if(other == null || other.owner == owner) return;

    	UnityEngine.Debug.Log(collider.gameObject);

    	if(priority >= 0 && (other.priority < 0 || priority <= other.priority))
    			Destroy(gameObject);
    }
}
