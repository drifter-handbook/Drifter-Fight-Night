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

    // Start is called before the first frame update
    void Start()
    {
        playerType = parent.GetComponent<NetworkSync>();
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
            if(OverrideData != null){
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, OverrideData);
            }
            else{
                hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, AttackData);
            }
            
        }
    }
}
