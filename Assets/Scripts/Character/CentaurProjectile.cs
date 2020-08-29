using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentaurProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    Rigidbody2D rb;
    float vel;
    void Start()
    {
        StartCoroutine(Fade(duration));
        rb = GetComponent<Rigidbody2D>();
        vel = rb.velocity.x;

    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(vel,rb.velocity.y);
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        if(col.gameObject.name == "Reflector"){
            rb.velocity =  rb.velocity * -2.5f;
            foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = col.gameObject.GetComponentInChildren<HitboxCollision>().parent;
                    //Mkae this not suck later
                    hitbox.AttackID = 10000;
                }

        }

    }

    IEnumerator Fade(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
