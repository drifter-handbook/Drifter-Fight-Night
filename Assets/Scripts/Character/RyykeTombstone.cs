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
    GameObject Ryyke;
    public bool grounded = false;

    // Start is called before the first frame update
    void Start()
    {
        Ryyke = chadController.gameObject.transform.parent.gameObject;
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

    void Update()
    {
        if (grounded)
        {
            rb.velocity = Vector2.zero;
        }
        anim.SetBool("Grounded",grounded);
    }

 	IEnumerator Arm()
    {
        yield return new WaitForSeconds(1.65f);
        armed = true;
        yield break;
    }


    void OnTriggerEnter2D(Collider2D col)
    {  

        if (col.gameObject.tag == "Ground")
        {
            grounded = true;  
            
            anim.SetBool("Grounded",true);
            rb.velocity=Vector2.zero;
            StartCoroutine(Arm());
            return;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {

        if(!armed && col.gameObject != Ryyke && col.gameObject != Ryyke.GetComponentInChildren<HurtboxCollision>().gameObject && col.gameObject.tag != "Ground"){
            anim.SetBool("Grounded",false);
            rb.velocity=Vector2.zero;
            Break();
        }

        else if(armed && col.gameObject != Ryyke && col.gameObject.tag == "Player") //&& col.gameObject != hitbox.parent)
        {
            anim.SetTrigger("Activate");
            StartCoroutine(Delete());
        }
    }  

    public void spawnChadWrapper(){
        chadController.SpawnChad(facing);
    }

}
