using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetGrabHitboxCollision : HitboxCollision
{

    public string SuccessState = "";
    public SyncAnimatorStateHost anim = null;

    void OnTriggerStay2D(Collider2D collider)
    {
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
    
        if (hurtbox != null && AttackType != DrifterAttackType.Null)
        {
            string player = playerType.NetworkType;
            if(OverrideData != null)
            {
                int hitResult = hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, -AttackID, AttackType, OverrideData);
                if(hitResult == 0 && SuccessState != "" && anim != null)
                       anim.SetState(SuccessState);
        
            }
            else{
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, AttackData);
            }
            
        }
    }
}