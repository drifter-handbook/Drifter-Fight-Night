 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstone : MonoBehaviour
{
    RykkeMasterHit chadController;
    Rigidbody2D rb;
    public Animator anim;
    public int facing = 0;
    bool armed = false;
    public bool activate = false;
    GameObject Ryyke;
    public bool grounded = false;
    public bool broken = false;

    // Start is called before the first frame update
    void Start()
    {
    	rb = GetComponent<Rigidbody2D>();
    	rb.velocity = new Vector2(0f,-50f);
    }

    public IEnumerator Delete()
    {

        yield return new WaitForSeconds(0.75f);
        Destroy(gameObject);
        yield break;
    }
    public void Break(){
        broken = true;
        anim.SetTrigger("Delete");
        StartCoroutine(Delete());
    }

    public void setChadController(RykkeMasterHit controller)
    {
        UnityEngine.Debug.Log("CHAD SET");
        chadController = controller;
    }

    void Update()
    {
        UnityEngine.Debug.Log("ChadController+" + chadController);
        if (grounded)
        {
            rb.velocity = Vector2.zero;
        }
        if(activate){
            anim.SetTrigger("Activate");
        }
        if(broken){
             anim.SetTrigger("Delete");
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

        if(Ryyke == null){
            Ryyke = chadController.gameObject.transform.parent.gameObject;
        }

        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Platform")
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

        if(!armed && col.gameObject != Ryyke && col.gameObject != Ryyke.GetComponentInChildren<HurtboxCollision>().gameObject && col.gameObject.tag != "Ground" && col.gameObject.tag != "Platform"){
            anim.SetBool("Grounded",false);
            rb.velocity=Vector2.zero;
            Break();
        }

        else if(armed && col.gameObject != Ryyke && col.gameObject.tag == "Player") //&& col.gameObject != hitbox.parent)
        {
            activate = true;
            anim.SetTrigger("Activate");
            StartCoroutine(Delete());
        }
    }  

    public void spawnChadWrapper(){
        chadController.SpawnChad(facing);
    }

}
