 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstone : MonoBehaviour
{
    public RykkeMasterHit chadController;
    Rigidbody2D rb;
    Animator anim;
    public int facing;
    bool armed = false;

    public bool grounded = false;

    // Start is called before the first frame update
    void Start()
    {
    	rb = GetComponent<Rigidbody2D>();
    	rb.velocity = new Vector2(0f,-50f);
        anim = GetComponent<Animator>();
    }

    public IEnumerator Delete()
    {
        yield return new WaitForSeconds(0.75f);
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

        UnityEngine.Debug.Log(col.gameObject.name);

        if (col.gameObject.tag == "Ground" && !grounded)
        {
        	grounded = true;  
            
			anim.SetBool("Grounded",true);
			rb.velocity=Vector2.zero;
			StartCoroutine(Arm());
		}

        else if(col.gameObject.name != chadController.gameObject.transform.parent.gameObject.name && !armed){
        	//anim.SetBool("Grounded",true);
        	grounded = true;
            anim.SetBool("Grounded",false);
        	rb.velocity=Vector2.zero;
        	Break();
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if(armed && col.gameObject.name != chadController.gameObject.transform.parent.gameObject.name && col.gameObject.tag == "Player") //&& col.gameObject != hitbox.parent)
        {
            anim.SetTrigger("Activate");
            StartCoroutine(Delete());
        }
    }

    public void spawnChadWrapper(){
        chadController.SpawnChad(facing);
    }

}
