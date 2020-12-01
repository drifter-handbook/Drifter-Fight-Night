using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnHitProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rb;
   

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.name == "Hurtboxes" && col.GetComponent<HurtboxCollision>() != this.gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponentInChildren<HurtboxCollision>())
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
