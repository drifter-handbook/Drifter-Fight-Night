using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxCollision : MonoBehaviour
{
    public GameObject parent;
    public NetworkSync playerType;

    public bool isActive = true;

    public int AttackID { get; set; }
    public DrifterAttackType AttackType { get; set; }
    public bool Active { get; set; } = true;
    public SingleAttackData OverrideData;
    public SingleAttackData AttackData;
    public int Facing { get; set; } = 1;

    Drifter drifter;

    // Start is called before the first frame update
    void Start()
    {
        playerType = parent.GetComponent<NetworkSync>();
        drifter = parent.GetComponent<Drifter>();
    }

    // Update is called once per frame
    void Update()
    {

    }


    //Ground Collision Sparks
    void OnTriggerEnter2D(Collider2D collider)
    {

    	if(collider.gameObject.tag == "Ground")
    	{

    		// if(collider.ClosestPoint(gameObject.transform.position) != (Vector2)gameObject.transform.position)
    		// {

    		// 	UnityEngine.Debug.Log("Sparks");
    		 GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.CollisionSpark,

    		  collider.ClosestPoint(gameObject.GetComponent<Collider2D>().ClosestPoint(collider.transform.position))


    		  , collider.gameObject.transform.rotation.eulerAngles.z, new Vector2(Facing * 1, 1));

    		// }

    	}
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
    
        if (hurtbox != null && AttackType != DrifterAttackType.Null && isActive)
        {
            //string player = playerType.NetworkType;
            int hitresult = -3;
            if(OverrideData != null){
                hitresult = hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, OverrideData);
            }
            else{
                hitresult = hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, AttackData);
            }
            if(hitresult >= -1 && drifter.canSpecialCancelFlag)drifter.listenForSpecialCancel = true;
            //Debug.Log("Hit result of: " + hitresult + "  For attack id: " + AttackID+ "  And AttackType: " + AttackType);
        }
    }
}
