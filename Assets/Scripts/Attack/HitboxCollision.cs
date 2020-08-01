using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxCollision : MonoBehaviour
{
    public GameObject parent;
    INetworkSync playerType;

    public int AttackID { get; set; }
    public DrifterAttackType AttackType { get; set; }

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
        Debug.Log("hi");
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
        if (hurtbox != null)
        {
            string player = playerType.Type;
            SingleAttackData attackData = GameController.Instance.AllData.GetAttacks(player)[AttackType];
            hurtbox.parent.GetComponent<PlayerAttacking>().RegisterAttackHit(this, hurtbox, AttackID, attackData);
        }
    }
}
