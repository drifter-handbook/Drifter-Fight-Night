using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeUppercutZombie : MonoBehaviour
{
    //public HitboxCollision hitbox;
    Rigidbody2D rb;
    //Animator anim;
    // Start is called before the first frame update
    void Start()
    {
    	rb = GetComponent<Rigidbody2D>();
    	rb.velocity= new Vector2(0f,40f);
        StartCoroutine(Delete());
    }

    public IEnumerator Delete()
    {
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
        yield break;
    }

    // Update is called once per frame
    void Update()
    {

    }

}
