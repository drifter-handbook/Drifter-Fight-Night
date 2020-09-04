using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OopsiePoopsie : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator anim;
    public Rigidbody2D rb;
    public bool empowered = false;
    public Collider2D hurtbox;
    public PlayerStatus status;

    void Start()
    {
    }

    void Update(){
        if(empowered)
        {
            anim.SetTrigger("Empower");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        UnityEngine.Debug.Log(col.gameObject.name);        
        if(col.gameObject.name == "Reflector"){
            rb.velocity =  rb.velocity * -2.5f;
            foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = col.gameObject.GetComponentInChildren<HitboxCollision>().parent;
                    //Mkae this not suck later
                    hitbox.AttackID = 10000;
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
