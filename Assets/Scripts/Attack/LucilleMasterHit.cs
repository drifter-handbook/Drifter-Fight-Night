using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucilleMasterHit : MasterHit
{


    public void Side_Attack_Fireball()
    {
        // jump upwards and create spear projectile
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(facing * 4.3f,3.7f,0);

        GameObject bolt = GameController.Instance.host.CreateNetworkObject("Lucille_Side_Fireball", transform.position + pos, transform.rotation);

         bolt.transform.localScale = new Vector3(facing * 10f,10f,0);

        bolt.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 20f,0f,0);
        foreach (HitboxCollision hitbox in bolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
            
        }
    }



    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -30f,0f);
    }

    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.velocity = new Vector3(0,70f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,0f);
    }
}
