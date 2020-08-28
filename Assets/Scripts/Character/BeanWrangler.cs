using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeanWrangler : MonoBehaviour
{
    // Start is called before the first frame update
    public bool hide;
    public int facing;
    public Animator anim;
    public PlayerAttacks attacks;
    public bool Up = false;
    public bool Down = false;
    public bool Side = false;
    public bool Neutral = false;
    public bool Hide = false;
    public OrroMasterHit orro;

    Rigidbody2D rb;

    void Start()
    {
        
    }

    IEnumerator delete(){
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
        yield break;
    }

    public void beanSpit(){
        orro.spawnBeanSpit(facing,gameObject.transform.position);
    }
 
    public void multihit(){
        orro.multihit();
    }

    void resetAnimatorTriggers(){
        Up = false;
        Down = false;
        Side = false;
        Neutral = false;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Hide", Hide);
        if (Side) anim.SetTrigger("Side");
        if (Down) anim.SetTrigger("Down");
        if (Up) anim.SetTrigger("Up");
        if (Neutral) anim.SetTrigger("Neutral");
   
        resetAnimatorTriggers();

    }
}
