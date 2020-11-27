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
        if(col.gameObject.name != "Reflector")
        {
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
