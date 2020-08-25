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
        UnityEngine.Debug.Log("MEGURIN HIT");
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
    
        if (hurtbox != null && AttackType != DrifterAttackType.Null)
        {
            if(useData != null){

                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, -AttackID, AttackType, Megurin.handleElements(useData, element));
            }
            else{
                this.attackData = GameController.Instance.AllData.GetAttacks("Megurin")[AttackType];
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, Megurin.handleElements(this.attackData, element));
            }
            
        }
    }
}
