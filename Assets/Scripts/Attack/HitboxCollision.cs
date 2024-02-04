using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxCollision : MonoBehaviour
{
    public int Facing { get; set; } = 1;
    public bool isActive { get; set; } = true;
    public int AttackID { get; set; }

    public SingleAttackData OverrideData;
    public GameObject parent;
    
    //-2 no interaction
    //-1 eat all projectiles, and faze throuhg other projectile eaters
    //0 eat all projectiles, but be be destroyed if hitting a 0 or a -1
    //1+ priority
    public int projectilePriority = -2;
    protected Drifter drifter;

    // Start is called before the first frame update
    void Start()
    {
        drifter = parent.GetComponent<Drifter>();
    }

    //Ground Collision Sparks
    void OnTriggerEnter2D(Collider2D collider)
    {

    	if(collider.gameObject.tag == "Ground")
    	{

    		 GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.CollisionSpark,

    		  collider.ClosestPoint(gameObject.GetComponent<Collider2D>().ClosestPoint(collider.transform.position))

    		  , collider.gameObject.transform.rotation.eulerAngles.z, new Vector2(Facing * 1, 1));

    	}
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.gameObject.layer != 10) return;
        //Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
        HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
        HitboxCollision hitbox = collider.GetComponent<HitboxCollision>();
    
        if (hurtbox != null && isActive)
        {
            //string player = playerType.NetworkType;
            int hitResult = -3;
            if(OverrideData != null){
                hitResult = (int)hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, OverrideData);
            }
            else{
                hitResult = (int)hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID, drifter.attacks.GetCurrentAttackData());
            }
            if(hitResult == 1) isActive = false;
            if(hitResult >= -1 && drifter.canSpecialCancelFlag)drifter.listenForSpecialCancel = true;
        }
        else if(hitbox != null && projectilePriority >= 0 && hitbox.projectilePriority>=-1)
        {
            if((projectilePriority ==0 && projectilePriority >= hitbox.projectilePriority) || (projectilePriority != 0  && projectilePriority <= hitbox.projectilePriority))
                Destroy(gameObject.transform.parent.gameObject);
        }
    }

    //Rollback
    //====================================
    
    //Takes a snapshot of the current frame to rollback to
    public HitboxRollbackFrame SerializeFrame()
    {
        return new HitboxRollbackFrame()
        {
            Facing = this.Facing,
            isActive = this.isActive,
            AttackID = this.AttackID,
            
        };
    }

    //Rolls back the entity to a given frame state
    public  void DeserializeFrame(HitboxRollbackFrame p_frame)
    {
        Facing = p_frame.Facing;
        isActive = p_frame.isActive;
        AttackID = p_frame.AttackID;
    }


}

public class HitboxRollbackFrame: INetworkData
{
    public string Type { get; set; }

    public int Facing;
    public bool isActive;
    public int AttackID;
    
}
