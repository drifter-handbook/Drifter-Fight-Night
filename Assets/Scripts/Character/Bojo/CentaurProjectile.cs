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

    IEnumerator Fade(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
