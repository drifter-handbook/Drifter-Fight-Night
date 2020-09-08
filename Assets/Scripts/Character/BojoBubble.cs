using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BojoBubble : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    public int mode;
    public Animator anim;
    Rigidbody2D rb;
    BojoSound sound;

    void Start()
    {
        StartCoroutine(Fade(duration));
        rb = GetComponent<Rigidbody2D>();
        sound = GetComponent<BojoSound>();
        sound.PlayAudio(mode);
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("Mode",mode);
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
        else if(col.gameObject.name == "Hurtboxes" && col.GetComponent<HurtboxCollision>() != this.gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponentInChildren<HurtboxCollision>())
        {
            StartCoroutine(Fade(.05f));
        }

    }

    IEnumerator Fade(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
