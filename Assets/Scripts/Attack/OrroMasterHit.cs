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
    float beanSpeed = 5f;
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

        if(!anim.GetBool("Empowered"))
        {
            localBean.GetComponent<BeanWrangler>().Hide = true;
            localBeanAnim.SetBool("Hide",true);
        }
        else
        {
            localBean.GetComponent<BeanWrangler>().Hide = false;
            localBeanAnim.SetBool("Hide",false);
        }
    }

    public void ledgeClimb(){
        facing = movement.Facing;
        rb.position += new Vector2(7f* facing,5.5f);
    }

    public void ledgeClimbLag(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.9f);
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
            hitbox.Facing = facing;
        }
        orroOrb.GetComponent<OrroSideWProjectile>().facing=facing;
        entities.AddEntity(orroOrb);
    }

    public void dodgeRoll()
    {
        beanSpeed = 5f;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.8f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.4f);
    }

    public void rollTele()
    {
        facing = movement.Facing;
        rb.position += new Vector2(facing* 10,0);
    }

	public void dair()
    {
    	facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x,35f);
        GameObject orroSplosion = Instantiate(entities.GetEntityPrefab("DairExplosion"), transform.position, transform.rotation);
        orroSplosion.transform.localScale = new Vector3(7.5f * facing,7.5f,1f);
        foreach (HitboxCollision hitbox in orroSplosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        entities.AddEntity(orroSplosion);
    }

    public void backdash(){
    	facing = movement.Facing;

    	rb.velocity = new Vector2(facing * -25f,23f);
    	beanSpeed = 5f;
    	if(anim.GetBool("Empowered")){
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
                hitbox.Facing = facing;
            }
            if(beanRemote){
                Destroy(beanRemote);
            }
            BeanProj.GetComponent<BeanWrangler>().facing=facing;
            beanRemote = BeanProj;
            localBean.GetComponent<BeanWrangler>().Hide = true;
            drifter.SetAnimatorBool("Empowered",false);
            entities.AddEntity(BeanProj);
        }
    }


    public void sair()
    {
    	facing = movement.Facing;
        GameObject marble = Instantiate(entities.GetEntityPrefab("Marble"), transform.position + new Vector3(0,3f,0), transform.rotation);
        foreach (HitboxCollision hitbox in marble.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        marble.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 35f, 0);
        entities.AddEntity(marble);
    }


    public void uair()
    {
    	facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x,10f);
        GameObject orroSplosion = Instantiate(entities.GetEntityPrefab("UairExplosion"), transform.position + new Vector3(0,.5f,0), transform.rotation);
        orroSplosion.transform.localScale = new Vector3(7.5f * facing,7.5f,1f);
        foreach (HitboxCollision hitbox in orroSplosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        entities.AddEntity(orroSplosion);
    }

     public void cancelGravity(){
        facing = movement.Facing;
        rb.velocity = Vector2.zero;
        movement.gravityPaused= true;
        rb.gravityScale = 0;
    }
    public void inTheHole(){
        
        rb.velocity = new Vector2(0f,225f);
    }

    public void resetGravity(){
        movement.gravityPaused= false;
        rb.gravityScale = gravityScale;
    }

    public void grabEndlag(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.55f);
    }
    //Bean

    public void chargebean()
    {
        beanSpeed+=10f;
        if(beanSpeed >= 55){
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
                hitbox.Facing = facing;
            }
            if(beanRemote){
                Destroy(beanRemote);
            }
            BeanProj.GetComponent<BeanWrangler>().facing=facing;
            beanRemote = BeanProj;
            localBean.GetComponent<BeanWrangler>().Hide = true;
            drifter.SetAnimatorBool("Empowered",false);
            entities.AddEntity(BeanProj);
            beanSpeed = 5f;
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
                hitbox.Facing = facing;
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
