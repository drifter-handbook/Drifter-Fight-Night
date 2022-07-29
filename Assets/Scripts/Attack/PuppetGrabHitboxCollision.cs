using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetGrabHitboxCollision : HitboxCollision
{

    public string SuccessState = "";
    public Animator animator = null;
    public GameObject victim;
    public bool playOnInvuln = false;
    public bool playOnBlock = false;

    void OnTriggerStay2D(Collider2D collider)
    {
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
        HitboxCollision hitbox = collider.GetComponent<HitboxCollision>();

        if (hurtbox != null && AttackType != DrifterAttackType.Null && isActive)
        {
            int hitResult = -3;
            //string player = playerType.NetworkType;
            if(OverrideData != null)
            {
                hitResult = hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID + 64, AttackType, OverrideData);
                if((hitResult == 0  || hitResult == 1) && SuccessState != "" && animator != null)
                {
                    victim = hurtbox.parent;
                    animator.Play(SuccessState);
                }
                else if((playOnInvuln && hitResult == -5))
                    animator.Play(SuccessState);
                else if((playOnBlock && (hitResult == -2 || hitResult == -1)))
                    animator.Play(SuccessState);
                       
            }
            else{
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, AttackData);
            }
            if(hitResult == 1) isActive = false;            
        }
        else if(hitbox != null && projectilePriority >= 0 && hitbox.projectilePriority>=-1)
        {
            if((projectilePriority ==0 && projectilePriority >= hitbox.projectilePriority) || (projectilePriority != 0  &&projectilePriority <= hitbox.projectilePriority))
                Destroy(gameObject.transform.parent.gameObject);
        }
    }
}