using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeanWrangler : MonoBehaviour
{
    // Start is called before the first frame update
    protected NetworkEntityList entities;
    public bool hide;
    public int facing;
    public Animator anim;
    public bool Up = false;
    public bool Down = false;
    public bool Side = false;
    public bool Neutral = false;
    public bool Hide = false;
    PlayerAttacks attacks;
    GameObject Orro;

    Rigidbody2D rb;

    void Start()
    {
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        Orro = gameObject.GetComponentInChildren<HitboxCollision>().parent;
        attacks = gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponent<PlayerAttacks>();
    }

    IEnumerator delete(){
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
        yield break;
    }

    public void beanSpit()
    {
        try{
        Vector3 flip = new Vector3(facing *8,8,0f);
        Vector3 pos = new Vector3(facing *.7f,2.5f,1f);
        GameObject spit = Instantiate(entities.GetEntityPrefab("BeanSpit"), transform.position + pos, transform.rotation);
        spit.transform.localScale = flip;
        

            attacks.SetMultiHitAttackID();
            spit.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 20, 0);
            foreach (HitboxCollision hitbox in spit.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = Orro;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
            }
            entities.AddEntity(spit);
        }
        finally{
            //Eventually
        }
    }
 
    public void multihit(){
        try{
            attacks.SetMultiHitAttackID();    
        }
        finally{
            //Eventually
        }
        
    }

    public void resetAnimatorTriggers(){
        Up = false;
        Down = false;
        Side = false;
        Neutral = false;
        anim.ResetTrigger("Side");
        anim.ResetTrigger("Down");
        anim.ResetTrigger("Up");
        anim.ResetTrigger("Neutral");
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Hide", Hide);
        if (Side) anim.SetTrigger("Side");
        if (Down) anim.SetTrigger("Down");
        if (Up) anim.SetTrigger("Up");
        if (Neutral) anim.SetTrigger("Neutral");

    }
}
