using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroSpear : MonoBehaviour
{
    public Vector2 velocity;

    Rigidbody2D rb;
    SpriteRenderer sr;

    public bool Active { get; set; } = true;
    public Sprite landed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<NeroSpearSync>().Active)
        {
            return;
        }
        if (Active)
        {
            rb.velocity = velocity;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(0.5f);
        float fadeDuration = 1f;
        for (float time = 0; time < fadeDuration; time += Time.deltaTime)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Lerp(1f, 0f, time / fadeDuration));
            yield return null;
        }
        Destroy(gameObject);
        yield break;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Ground")
        {
            if (Active)
            {
                StartCoroutine(Fade());
            }
            Active = false;
            sr.sprite = landed;
        }
    }
}
