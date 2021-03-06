using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinHitboxCollision : HitboxCollision
{
    MegurinMasterHit Megurin;
    public int element = 2;

    void Start()
    {
        Megurin = this.parent.GetComponentInChildren<MegurinMasterHit>();
    }

    // Start is called before the first frame update   
    void OnTriggerStay2D(Collider2D collider)
    {
        if (!GameController.Instance.IsHost)
        {
            return;
        }
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        if(Megurin!= null)
        {
            HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();    
            if (hurtbox != null
                && hurtbox != this.parent.GetComponentInChildren<HurtboxCollision>()
                && AttackType != DrifterAttackType.Null
                && !hurtbox.parent.GetComponent<PlayerHurtboxHandler>().oldAttacks.ContainsKey(AttackID)
                && !hurtbox.parent.GetComponent<PlayerHurtboxHandler>().oldAttacks.ContainsKey(-AttackID)
                && !hurtbox.parent.GetComponent<PlayerStatus>().HasStatusEffect(PlayerStatusEffect.INVULN))
            {

                if(OverrideData != null){

                    hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, -AttackID, AttackType, Megurin.handleElements(OverrideData, element));
                }
                else{
                    hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, Megurin.handleElements(this.AttackData, element));
                }
                
            }
        }
    }
}
