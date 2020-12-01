using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroSideWProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    public int facing;
    public Animator anim;
    public Rigidbody2D rb;
    public bool empowered = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.name == "Hurtboxes" && !empowered && col.GetComponent<HurtboxCollision>() != this.gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponentInChildren<HurtboxCollision>())
        {
            StartCoroutine(delete(.1f));
        }
    }
    
    public void empower()
    {
        empowered = true;
        rb.velocity += new Vector2(facing *15f,0f);
    }

    IEnumerator delete(float time){
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
