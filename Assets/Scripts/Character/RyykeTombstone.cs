 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstone : MonoBehaviour
{
    protected NetworkEntityList entities;
    Rigidbody2D rb;
    public Animator anim;
    public int facing = 0;
    bool armed = false;
    public bool activate = false;
    public GameObject Ryyke;
    public PlayerAttacks attacks;
    public bool grounded = false;
    public bool broken = false;

    // Start is called before the first frame update
    void Start()
    {
    	rb = GetComponent<Rigidbody2D>();
    	rb.velocity = new Vector2(0f,-50f);
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        Ryyke = gameObject.GetComponentInChildren<HitboxCollision>().parent;
        UnityEngine.Debug.Log("Ryyke:" + Ryyke);
        attacks = Ryyke.GetComponent<PlayerAttacks>();
        UnityEngine.Debug.Log( "PlayerAtta:" + attacks);

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

    void Update()
    {
        if (grounded)
        {
            rb.velocity = Vector2.zero;
        }
        if(activate){
            anim.SetTrigger("Activate");
            StartCoroutine(Delete());
        }
        if(broken){
             anim.SetTrigger("Delete");
             StartCoroutine(Delete());
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

    public void SpawnChad(){
        Vector3 flip = new Vector3(facing *8f,8f,1f);
        GameObject zombie = Instantiate(entities.GetEntityPrefab("Chadwick"), transform.position, transform.transform.rotation);
        
        try{
             zombie.transform.localScale = flip;        
            foreach (HitboxCollision hitbox in zombie.GetComponentsInChildren<HitboxCollision>(true))
            {
                attacks.SetupAttackID(DrifterAttackType.W_Down);
                hitbox.parent = Ryyke;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = DrifterAttackType.W_Down;
                hitbox.Active = true;
            }
            Ryyke.GetComponentInChildren<RykkeMasterHit>().grantStack();
            entities.AddEntity(zombie);
        }
        finally
        {
            //I'm sick of this shit
        }

    }  
}
