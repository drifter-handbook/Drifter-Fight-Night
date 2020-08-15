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
    void Start()
    {

        StartCoroutine(Empower());
        StartCoroutine(delete());
        
    }
    IEnumerator Empower(){
        yield return new WaitForSeconds(duration * .50f);
        anim.SetTrigger("Empower"); 
        rb.velocity += new Vector2(facing *15f,0f);

        yield break;
    }

    IEnumerator delete(){
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
        yield break;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
