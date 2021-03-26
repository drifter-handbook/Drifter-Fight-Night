using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabHitboxCollision : HitboxCollision
{
    // Start is called before the first frame update


	public string SuccessState = "";

	public GameObject victim;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

      void OnTriggerStay2D(Collider2D collider)
    {
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
    
        if (hurtbox != null && AttackType != DrifterAttackType.Null)
        {
            //string player = playerType.NetworkType;
            if(OverrideData != null){
                int hitResult = hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, OverrideData);
                if(hitResult >=0)
                {
                	victim = hurtbox.parent;
                    if(SuccessState != ""){
                        parent.GetComponent<Drifter>().movement.canLandingCancel = false;
                        parent.GetComponent<Drifter>().PlayAnimation(SuccessState);
                    }
                }
            }
            else{
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, AttackData);
            }
            
        }
    }
}
