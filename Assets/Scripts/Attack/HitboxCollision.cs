using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxCollision : MonoBehaviour
{
    public GameObject parent;
    protected INetworkSync playerType;

    public int AttackID { get; set; }
    public DrifterAttackType AttackType { get; set; }
    public bool Active { get; set; } = true;
    public SingleAttackData OverrideData;
    public SingleAttackData AttackData;
    public int Facing { get; set; } = 1;

    // Start is called before the first frame update
    void Start()
    {
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
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
    
        if (hurtbox != null && AttackType != DrifterAttackType.Null)
        {
            string player = playerType.Type;
            if(OverrideData != null){
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, OverrideData);
            }
            else{
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, AttackData);
            }
            
        }
    }
}
