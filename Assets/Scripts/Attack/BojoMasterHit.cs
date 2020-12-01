using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BojoMasterHit : MasterHit
{

    GameObject Centaur = null;

    public void GUN(){

        if(!isHost)return;

        //applyEndLag(drifter.input.Special?0:1);
        Empowered = drifter.input.Special;
        
    	facing = movement.Facing;
        Vector3 flip = new Vector3(facing *6f,6f,0f);
        Vector3 pos = new Vector3(facing *3f,4f,1f);
        GameObject bubble = host.CreateNetworkObject("Mockery", transform.position + pos, transform.rotation);
        bubble.transform.localScale = flip;
        bubble.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 55, 0);
        refreshHitboxID();
        foreach (HitboxCollision hitbox in bubble.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
    }

    public void dismount()
    {
        if(!isHost)return;
        facing = movement.Facing;
        if(TransitionFromChanneledAttack())
        {

            if(Centaur != null)Destroy(Centaur);
            Vector3 flip = new Vector3(facing *9f,9f,0f);
            Vector3 pos = new Vector3(facing *0f,0f,1f);
            Centaur = host.CreateNetworkObject("Kamikaze", transform.position + pos, transform.rotation);
            Centaur.transform.localScale = flip;
            Centaur.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 50, 0);
        
            foreach (HitboxCollision hitbox in Centaur.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
        }

    }

    public void upWGlide()
    {
        if(!isHost || TransitionFromChanneledAttack())return;

        if(drifter.input.MoveY <0 || movement.grounded)returnToIdle();
        else
        {
            movement.updateFacing();
            rb.velocity = new Vector2(drifter.input.MoveX * 15f,rb.velocity.y*.5f);
        }
    }

    //Inhereted Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0,70f,0);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.42f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,5f);
    }

}
