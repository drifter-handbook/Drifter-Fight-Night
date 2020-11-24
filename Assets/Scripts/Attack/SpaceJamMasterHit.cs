using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceJamMasterHit : MasterHit
{
   
    public SpriteRenderer sprite;
    public Animator anim;
    int charges = 0;
    int maxCharge = 65;

    public AudioSource audioSource;
    public AudioClip[] audioClips;


    //Side W
    public void GuidingBolt()
    {
        audioSource.PlayOneShot(audioClips[1], 1f);
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *-3f,0f,1f);
        GameObject GuidingBolt = Instantiate(entities.GetEntityPrefab("GuidingBolt"), transform.position + pos, transform.rotation);
        GuidingBolt.transform.localScale = flip;
        GuidingBolt.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * -30, 0);
        foreach (HitboxCollision hitbox in GuidingBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        entities.AddEntity(GuidingBolt);       
            
    }


    //Down W
    public void oopsiePoopsie()
    {
        facing = movement.Facing;
        rb.velocity += new Vector2(-20*facing,0);
        if(anim.GetBool("Empowered")){
            drifter.SetAnimatorBool("Empowered",false);
            sprite.color = Color.white;
            audioSource.PlayOneShot(audioClips[1], 1f);
            Vector3 flip = new Vector3(facing *12f,12f,0f);
            Vector3 pos = new Vector3(facing *-3f,0f,1f);
            GameObject amber = Instantiate(entities.GetEntityPrefab("Amber"), transform.position + pos, transform.rotation);
            amber.transform.localScale = flip;
            amber.GetComponent<Rigidbody2D>().velocity = rb.velocity;
            foreach (HitboxCollision hitbox in amber.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
            amber.GetComponent<OopsiePoopsie>().hurtbox = gameObject.transform.Find("Hurtboxes").gameObject.GetComponent<CapsuleCollider2D>();
            amber.GetComponent<OopsiePoopsie>().status = status;
        entities.AddEntity(amber);
        charges = 0;
        }
    }


    //Neutral W Charge

    public void chargeOopsie(int cancelable)
    {
        if(TransitionFromChanneledAttack() && cancelable != 0)
        {
            return;
        }
        else
        {
            if(charges < maxCharge)
            {
                charges++;
            }
        
            if(drifter.DamageTaken >= .2f)
            {
                drifter.DamageTaken -= .2f;
            }
            else
            {
                    drifter.DamageTaken = 0f;
            }

            if(charges >= maxCharge)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(audioClips[2],1f);
                    drifter.SetAnimatorBool("Empowered",true);
                    sprite.color = new Color(255,165,0);
            }
        }

    }

    //Inherited Dodge roll methods

    public override void roll(){
        audioSource.Pause();
        facing = movement.Facing;
        applyEndLag(1f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,0f);
    }

     public override void rollGetupStart(){
        applyEndLag(1);
        rb.velocity = new Vector3(0f,45f,0);
    }

    public override void rollGetupEnd()
    {
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,5f);
    }

}
