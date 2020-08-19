using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxCollision : MonoBehaviour
{
    public GameObject parent;
    INetworkSync playerType;

    public int AttackID { get; set; }
    public DrifterAttackType AttackType { get; set; }
    public bool Active { get; set; } = false;
    public SingleAttackData useData;
    SingleAttackData attackData;

    // Start is called before the first frame update
    CapsuleCollider2D capsule;
    void Start()
    {
        capsule = GetComponentInChildren<CapsuleCollider2D>();
        playerType = parent.GetComponent<INetworkSync>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        //Debug.Log("hi45");
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
    
        if (hurtbox != null && AttackType != DrifterAttackType.Null)
        {
            string player = playerType.Type;
            if(useData != null){
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, -AttackID, AttackType, useData);
            }
            else{
                attackData = GameController.Instance.AllData.GetAttacks(player)[AttackType];
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, attackData);
            }
            
        }
    }
}
