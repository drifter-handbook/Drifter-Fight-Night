using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabHitboxCollision : HitboxCollision
{
    // Start is called before the first frame update

	public string SuccessState = "";
    public bool cancelable = true;

    void OnTriggerStay2D(Collider2D collider) {
        if(collider.gameObject.layer != 10) return;
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
        HitboxCollision hitbox = collider.GetComponent<HitboxCollision>();
    
        if (hurtbox != null && isActive)
        {
            //string player = playerType.NetworkType;
            int hitResult = -5;
            if(OverrideData != null){
                hitResult = (int)hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, OverrideData);
                if(hitResult == 1 || hitResult == 0)
                {
                    if(SuccessState != ""){
                        parent.GetComponent<Drifter>().movement.canLandingCancel = false;
                        parent.GetComponent<Drifter>().PlayAnimation(SuccessState,-1,true);
                    }

                }
            }
            else{
                hitResult = (int)hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, drifter.attacks.GetCurrentAttackData());
            }
            if(hitResult >= -1 && cancelable && drifter.canSpecialCancelFlag)drifter.listenForSpecialCancel = true;
            if(hitResult == 1) isActive = false;
        }
        else if(hitbox != null && projectilePriority >= 0 && hitbox.projectilePriority>=-1)
        {
            if((projectilePriority ==0 && projectilePriority >= hitbox.projectilePriority) || (projectilePriority != 0  &&projectilePriority <= hitbox.projectilePriority))
                Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
