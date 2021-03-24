using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnHitProjectile : MonoBehaviour
{
    // Start is called before the first frame update


     Rigidbody2D rb;

    void Start()
    {
        if(!GameController.Instance.IsHost)return;
        rb = GetComponent<Rigidbody2D>();
    }

   
   

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!GameController.Instance.IsHost)return;
        if(col.gameObject.name == "Hurtboxes" && col.GetComponent<HurtboxCollision>() != this.gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponentInChildren<HurtboxCollision>())
        {
            if(rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.gravityScale = 0;
            }
            StartCoroutine(Fade(.1f));
        }

    }

    IEnumerator Fade(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
