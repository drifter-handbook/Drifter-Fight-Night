using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    Rigidbody2D rb;
    void Start()
    {
        StartCoroutine(Fade(duration));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {

        if(col.gameObject.tag == "Reflector"){
            rb.velocity =  rb.velocity * -1.5f;
        }
        else if(col.gameObject.name == "Hurtboxes")
        {
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
