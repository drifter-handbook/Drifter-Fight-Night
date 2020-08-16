using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictBoxCollision : HitboxCollision
{
    INetworkSync playerType;
    RykkeMasterHit masterHit;

    public int AttackID { get; set; }
    public DrifterAttackType AttackType { get; set; }
    public bool Active { get; set; } = false;

    // Start is called before the first frame update
    CapsuleCollider2D capsule;
    void Start()
    {
        capsule = GetComponentInChildren<CapsuleCollider2D>();
        playerType = parent.GetComponent<INetworkSync>();
        masterHit = parent.GetComponentInChildren<RykkeMasterHit>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("hi22");
        Vector3 position = collider.transform.position;
        Debug.Log("hey new position22222" + position);
        masterHit.updatePosition(position);
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
        if (Active && hurtbox != null && AttackType != DrifterAttackType.Null)
        {
            Vector3 position = collider.transform.position;
            Debug.Log("hey new positiona" + position);
            masterHit.updatePosition(position);
            //string player = playerType.Type;
            //SingleAttackData attackData = GameController.Instance.AllData.GetAttacks(player)[AttackType];
            //hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, attackData);
        }
    }
}
