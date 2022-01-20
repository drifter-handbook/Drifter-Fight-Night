using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxCollision : MonoBehaviour
{
    public GameObject parent;
    public NetworkSync playerType;

    public bool destroyParentOnHit = false;
    public int AttackID { get; set; }
    public DrifterAttackType AttackType { get; set; }
    public bool isActive { get; set; } = true;
    public SingleAttackData OverrideData;
    public SingleAttackData AttackData;
    public int Facing { get; set; } = 1;

    //-2 no interaction
    //-1 eat all projectiles, and faze throuhg other projectile eaters
    //0 eat all projectiles, but be be destroyed if hitting a 0 or a -1
    //1+ priority
    public int projectilePriority = -2;

    protected Drifter drifter;

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
        HitboxCollision hitbox = collider.GetComponent<HitboxCollision>();
    
        if (hurtbox != null && AttackType != DrifterAttackType.Null && isActive)
        {
            //string player = playerType.NetworkType;
            int hitResult = -3;
            if(OverrideData != null){
                hitResult = hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, OverrideData);
            }
            else{
                hitResult = hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, AttackType, AttackData);
            }
            if(hitResult == 1) isActive = false;
            if(hitResult >= -1 && drifter.canSpecialCancelFlag)drifter.listenForSpecialCancel = true;
            if(hitResult >= -2 && destroyParentOnHit)Destroy(gameObject.transform.parent.gameObject);
            //Debug.Log("Hit result of: " + hitResult + "  For attack id: " + AttackID+ "  And AttackType: " + AttackType);
        }
        else if(hitbox != null && projectilePriority >= 0 && hitbox.projectilePriority>=-1)
        {
            if((projectilePriority ==0 && projectilePriority >= hitbox.projectilePriority) || (projectilePriority != 0  &&projectilePriority <= hitbox.projectilePriority))
                Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
