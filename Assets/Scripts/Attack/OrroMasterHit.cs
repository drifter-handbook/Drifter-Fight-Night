using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public int facing;
    public Animator anim;
    GameObject beanRemote;
    public GameObject localBean;
    Animator localBeanAnim;
    PlayerStatus status;
    float beanSpeed = 10f;
    LayerMask aLayerMask;

    void Start()
    {

        localBeanAnim = localBean.GetComponent<Animator>();
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
    }
    void Update(){
        localBean.GetComponent<BeanWrangler>().facing =  movement.Facing;
        if(beanRemote != null)
        {
            localBean.GetComponent<BeanWrangler>().Hide = true;
        }
        else if(beanRemote == null)
        {
            localBean.GetComponent<BeanWrangler>().Hide = false;
        }
    }

    public void spawnFireball()
    {
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *3f,3.5f,1f);
        GameObject orroOrb = Instantiate(entities.GetEntityPrefab("OrroSideW"), transform.position + pos, transform.rotation);
        orroOrb.transform.localScale = flip;
        orroOrb.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 25, 0);
        foreach (HitboxCollision hitbox in orroOrb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        orroOrb.GetComponent<OrroSideWProjectile>().facing=facing;
        entities.AddEntity(orroOrb);
    }

    public void dodgeRoll()
    {
        beanSpeed = 10f;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.8f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.4f);
    }

    public void rollTele()
    {
        facing = movement.Facing;
        rb.position += new Vector2(facing* 10,0);
    }

     public void cancelGravity(){
        facing = movement.Facing;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
    }
    public void inTheHole(){
        facing = movement.Facing;
        rb.velocity = Vector2.zero;
        aLayerMask = ~(1 << LayerMask.NameToLayer ("Player") | 1 << LayerMask.NameToLayer ("Platform"));
        RaycastHit2D hit = Physics2D.Raycast(rb.position, new Vector2(0,1), 20, aLayerMask);
        if(hit.collider != null && hit.collider.gameObject!= null && hit.collider.gameObject.tag != "Untagged")
        {
            Debug.Log("Hit this" + hit.collider);
            Debug.Log("Did Hit" + hit.distance);
            var distance = hit.distance;
            if (distance <4f){
              distance = 0;
            }
            rb.position += new Vector2(0, distance);
        }
        else{
          rb.position += new Vector2(0,20);
        }
    }

    public void resetGravity(){
        rb.gravityScale = gravityScale;
    }

    public void grabEndlag(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.55f);
    }
    //Bean

    public void chargebean()
    {
        beanSpeed+=10f;
        if(beanSpeed >= 50){
            drifter.SetAnimatorTrigger("W_Neutral");
        }
    }

    public void startFireBean(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.33f);
    }

    public void fireBean()
    {
        if(anim.GetBool("Empowered")){
            facing = movement.Facing;
            Vector3 flip = new Vector3(facing *6.7f,6.7f,0f);
            Vector3 pos = new Vector3(facing *1.3f,2f,1f);
            GameObject BeanProj = Instantiate(entities.GetEntityPrefab("Bean"), transform.position + pos, transform.rotation);
            BeanProj.transform.localScale = flip;
            BeanProj.GetComponent<Rigidbody2D>().simulated = true;
            BeanProj.GetComponent<Rigidbody2D>().velocity = new Vector2(facing *beanSpeed, 0f);
            foreach (HitboxCollision hitbox in BeanProj.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
            }
            if(beanRemote){
                Destroy(beanRemote);
            }
            BeanProj.GetComponent<BeanWrangler>().facing=facing;
            beanRemote = BeanProj;
            localBean.GetComponent<BeanWrangler>().Hide = true;
            drifter.SetAnimatorBool("Empowered",false);
            entities.AddEntity(BeanProj);
            beanSpeed = 20f;
        }
        else{
            BeanRecall();
        }

    }

    public void jabCombo()
    {
        attacks.SetupAttackID(DrifterAttackType.Ground_Q_Neutral);
    }


    public void multihit()
    {
        attacks.SetMultiHitAttackID();
    }


    public void BeanSide()
    {
        beanAttack("Side").GetComponent<BeanWrangler>().Side = true;
    }
    public void BeanDown()
    {
        beanAttack("Down").GetComponent<BeanWrangler>().Down = true;
    }
    public void BeanUp()
    {
        beanAttack("Up").GetComponent<BeanWrangler>().Up = true;
    }
    public void BeanNeutral()
    {
        beanAttack("Neutral").GetComponent<BeanWrangler>().Neutral = true;
    }

    private GameObject beanAttack(string direction)
    {
        attacks.SetMultiHitAttackID();

        if(beanRemote){
            refreshBeanHitboxes(beanRemote);
            return beanRemote;
        }
        else{
           refreshBeanHitboxes(localBean);
           return localBean;
        }

    }

    private void refreshBeanHitboxes(GameObject bean){
        bean.GetComponent<BeanWrangler>().resetAnimatorTriggers();
        foreach (HitboxCollision hitbox in bean.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = DrifterAttackType.W_Neutral;
                hitbox.Active = true;
            }
    }

    public void BeanRecall()
    {
        Destroy(beanRemote);
        localBean.GetComponent<BeanWrangler>().Hide = false;
        drifter.SetAnimatorBool("Empowered",true);
        beanRemote = null;
    }


}
