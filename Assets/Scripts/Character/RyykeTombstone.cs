 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstone : MonoBehaviour
{
    public HitboxCollision hitbox;
	public Vector2 velocity;
    public RykkeMasterHit chadController;
    Rigidbody2D rb;
    Animator anim;
    public int facing;
    bool armed = false;

    public bool grounded { get; set; } = false;

    // Start is called before the first frame update
    void Start()
    {
    	rb = GetComponent<Rigidbody2D>();
    	rb.velocity=velocity;
        anim = GetComponent<Animator>();
    }

    public IEnumerator Delete()
    {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
        yield break;
    }
    public void Break(){
        anim.SetTrigger("Delete");
        StartCoroutine(Delete());
    }

 	IEnumerator Arm()
    {
        yield return new WaitForSeconds(1.5f);
        armed = true;
        yield break;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Ground" && !grounded)
        {
        	grounded = true;  
			anim.SetBool("Grounded",true);
			grounded = true;
			rb.velocity=Vector2.zero;
			StartCoroutine(Arm());
		}
        else if(col.gameObject.tag != "Ground"  && col.gameObject != hitbox.parent && !grounded){
        	//anim.SetBool("Grounded",true);
        	grounded = true;
        	rb.velocity=Vector2.zero;
        	Break();
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if(armed && col.gameObject.tag == "Player" && col.gameObject != hitbox.parent){  //&& col.gameObject != hitboxParent){
            anim.SetTrigger("Activate");
            StartCoroutine(Delete());
        }
    }

    public void spawnChadWrapper(){
        chadController.SpawnChad(facing);
    }

}
