using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OopsiePoopsie : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator anim;
    public Rigidbody2D rb;
    public bool empowered = false;
    Collider2D hurtbox;
    PlayerStatus status;

    void Start()
    {
        hurtbox = gameObject.GetComponentInChildren<PuppetHitboxCollision>().parent.GetComponentInChildren<HurtboxCollision>().gameObject.GetComponentInChildren<CapsuleCollider2D>();
        status = gameObject.GetComponentInChildren<PuppetHitboxCollision>().parent.GetComponentInChildren<PlayerStatus>();
    }

    void Update(){
        if(empowered)
        {
            anim.SetTrigger("Empower");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {

         if(col.gameObject.name == "Reflector"){
            rb.velocity =  rb.velocity * -1.5f;

            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1,gameObject.transform.localScale.y,gameObject.transform.localScale.z);

            foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = col.gameObject.transform.parent.GetComponentInChildren<HitboxCollision>().parent;
                    //Mkae this not suck laters
                    hitbox.AttackID = 300 + Random.Range(0,25);
                }

        }
        else{
            anim.SetTrigger("Empower");
            empowered = true;
            rb.gravityScale = 0f;
            rb.velocity = Vector3.zero;
            StartCoroutine(delete(.4f));
        }
    }
    void OnTriggerStay2D(Collider2D col){
        if(col == hurtbox){
            status.ApplyStatusEffect(PlayerStatusEffect.AMBERED,3f);
        }
    }

    IEnumerator delete(float time){
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
