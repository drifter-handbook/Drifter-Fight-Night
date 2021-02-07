using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceJamMasterHit : MasterHit
{
 
    public SpriteRenderer sprite;
    int charges = 0;
    int maxCharge = 65;

    public AudioSource audioSource;
    public AudioClip[] audioClips;


    //Side W
    public void GuidingBolt()
    {
        audioSource.PlayOneShot(audioClips[1], 1f);
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *-3f,0f,1f);
        
        GameObject GuidingBolt = host.CreateNetworkObject("GuidingBolt", transform.position + pos, transform.rotation);
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
    }


    //Down W
    public void oopsiePoopsie()
    {
        if(!isHost)return;

        facing = movement.Facing;
        
        rb.velocity += new Vector2(-20*facing,0);
        if(!Empowered)return;

        Empowered = false;
        drifter.SetCharge(0);

        drifter.AirIdleStateName = "Hang";
        drifter.WalkStateName = "Walk";
        drifter.GroundIdleStateName = "Idle";

        
        audioSource.PlayOneShot(audioClips[1], 1f);
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *-3f,0f,1f);
            
        GameObject amber = host.CreateNetworkObject("Amber", transform.position + pos, transform.rotation);
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
        charges = 0;
    }


    //Neutral W Charge

    public void chargeOopsie()
    {
        if(!isHost)return;
        if(!cancelAttack())
        {
            if(charges < maxCharge)
            {
                charges++;
                if(charges % 3 == 0)GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Heal, transform.position + new Vector3(UnityEngine.Random.Range(-1.5f,2.5f), UnityEngine.Random.Range(-1.5f,1f)), 0, new Vector2(1, 1));
            }
            
            if(drifter.DamageTaken >= .2f) drifter.DamageTaken -= .2f;

            else drifter.DamageTaken = 0f;

            if(charges >= maxCharge)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(audioClips[2],1f);
                Empowered = true;
                drifter.SetCharge(4);

                drifter.AirIdleStateName = "Amber_Hang";
                drifter.WalkStateName = "Amber_Walk";
                drifter.GroundIdleStateName = "Amber_Idle";
                drifter.PlayAnimation("W_Neutral_Finish");
                //sprite.color = new Color(255,165,0);
            }
        }

    }

    //Inherited Dodge roll methods

    public override void roll(){
        audioSource.Pause();
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,0f);
    }

    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.velocity = new Vector3(0f,45f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,5f);
    }

}
