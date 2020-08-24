using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCollision : HitboxCollision
{
    INetworkSync playerType;
    SingleAttackData attackData;
    MegurinMasterHit megurin;
    // Start is called before the first frame update
    CapsuleCollider2D capsule;
    int iceCharge;
    void Start()
    {
        capsule = GetComponentInChildren<CapsuleCollider2D>();
        playerType = parent.GetComponent<INetworkSync>();
        megurin = parent.GetComponentInChildren<MegurinMasterHit>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        //Debug.Log("hi45");
        megurin.addIce();
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
        iceCharge = megurin.getIce();
        Debug.Log("got it backo " + iceCharge);
        if (hurtbox != null && AttackType != DrifterAttackType.Null)
        {
            string player = playerType.Type;
            if(useData != null){
                if (iceCharge >=6){
                  hurtbox.parent.GetComponent<PlayerMovement>().registerFreeze();
                  megurin.resetIce();
                }
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, -AttackID, AttackType, useData);
            }
            //else{
                //attackData = GameController.Instance.AllData.GetAttacks(player)[AttackType];
                //Debug.Log("stun " + attackData.HitStun);
                //hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, attackData);
            //}
        }
    }
}
