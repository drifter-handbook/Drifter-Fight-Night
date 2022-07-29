using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetHitboxCollision : HitboxCollision
{

    void OnTriggerStay2D(Collider2D collider)
    {
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
        HitboxCollision hitbox = collider.GetComponent<HitboxCollision>();
    
        int hitResult = -3;
        if (hurtbox != null && AttackType != DrifterAttackType.Null && isActive)
        {
            //string player = playerType.NetworkType;
            hitResult = hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID + 64, AttackType, OverrideData != null?OverrideData:AttackData);
     
            if(hitResult == 1) isActive = false;
            
        }
        else if(hitbox != null && projectilePriority >= 0 && hitbox.projectilePriority>=-1)
        {
            if((projectilePriority ==0 && projectilePriority >= hitbox.projectilePriority) || (projectilePriority != 0  &&projectilePriority <= hitbox.projectilePriority))
                Destroy(gameObject.transform.parent.gameObject);
        }
    }
}