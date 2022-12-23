using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherRollbackFrame: INetworkData
{
    public string Type { get; set; }

    public Vector2 Target;
    public Vector2 TargetOffset;
    public float Speed;
    public int ExtendPercent;
    public BasicProjectileRollbackFrame Projectile;
    public bool IsActive; 
}

public class Tether : MonoBehaviour
{

	//protected static float framerateScalar =.0833333333f;
	private Vector2 target = new Vector2(.24f,.16f);
	private Vector2 targetOffset = new Vector2(.2f,0);
	private float speed = 2f;
	private int extendPercent = 0;
    public bool isActive = true; 

	public SpriteRenderer sprite;
    public Collider2D hitbox;
    
    // Update is called once per frame
    public void UpdateFrame()
    {
        sprite.size = Vector2.MoveTowards(sprite.size,target,extendPercent);
        hitbox.offset = Vector2.MoveTowards(hitbox.offset,targetOffset,extendPercent);

        if(extendPercent < 100) extendPercent =  (int)Mathf.Min(extendPercent + speed,100);

        GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
        GetComponentInChildren<HitboxCollision>().isActive = isActive;
    }

    public void setTargetLength(float len)
    {
    	extendPercent = 0;
    	target = new Vector2(len,.16f);
    	targetOffset = new Vector2(len,0);
    }

    public void setSpeed(float spd)
    {
    	speed = spd;
    }

    public void freezeLen()
    {
    	target = sprite.size;
    	targetOffset = hitbox.offset;
    	extendPercent = 100;
    }

    public void togglehitbox(int active)
    {
    	isActive = (active != 0);
    }


    //Takes a snapshot of the current frame to rollback to
    public TetherRollbackFrame SerializeFrame()
    {
        return new TetherRollbackFrame() 
        {
            Target = target,
            TargetOffset = targetOffset,
            Speed = speed,
            ExtendPercent = extendPercent,
            Projectile = GetComponent<InstantiatedEntityCleanup>().SerializeFrame(),
            IsActive = isActive
        };
    }

    //Rolls back the entity to a given frame state
    public void DeserializeFrame(TetherRollbackFrame p_frame)
    {
        target = p_frame.Target;
        targetOffset = p_frame.TargetOffset;
        speed = p_frame.Speed;
        extendPercent = p_frame.ExtendPercent;

        GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(p_frame.Projectile);
        isActive = p_frame.IsActive;
    }
}
