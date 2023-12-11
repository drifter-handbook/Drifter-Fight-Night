using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NonplayerHurtboxHandler : PlayerHurtboxHandler
{

    protected bool takesKnockback = true;
    public float maxPercentage = 30f;
    public float _percentage = 0f;
	public float percentage
    {
        get{ return _percentage;}
        set
        {
            if(_percentage != value)
            {
                    _percentage = value;
                    healthBar?.updateHealthbar((maxPercentage - _percentage) / maxPercentage);
            }
        }
    }

    private int _facing = 1;
    public int facing
    {
        get { return _facing;}
        set {
            _facing = value;
            if(healthBar!=null)
                healthBar.facing = value;
        }
    }

    protected int HitstunDuration = 0;
    protected int HitPauseDuration = 0;
    protected Vector3 delayedVelocity = Vector3.zero;

    public Rigidbody2D rb;

    public SummonHealthbarHandler healthBar;

    new void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void UpdateFrame()
    {
        base.UpdateFrame();

        if(HitPauseDuration > 0)
        {
            HitPauseDuration--;
            if(takesKnockback && delayedVelocity != Vector3.zero && HitPauseDuration <=0)
            {
                rb.velocity = delayedVelocity;
                delayedVelocity = Vector3.zero;
            }
        }

        else if(HitstunDuration > 0)
        {
            HitstunDuration--;
            if(HitstunDuration < 0) HitstunDuration = 0;
        }

                    

    }

	public override int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, SingleAttackData attackData)
    {

        if (hitbox.parent != hurtbox.parent && hurtbox.owner != hitbox.parent && CanHit(attackID))
        {
            
            // register new attack
            oldAttacks[attackID] = MAX_ATTACK_DURATION;

            Vector3 hitSparkPos = hurtbox.capsule.ClosestPoint(hitbox.parent.transform.position);

            PlayerStatus attackerStatus = hitbox.parent.GetComponent<PlayerStatus>();


            if(hurtbox.gameObject.name == "Counter" &&  attackData.AttackDamage >0f && attackData.hitType!=HitType.GRAB)
            {
                GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.STAR, hitSparkPos,0, new Vector2(10f, 10f));
                attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,48);
                gameObject.GetComponent<SyncAnimatorStateHost>().SetState("Counter_Success");
                return -4;
            }


    	    percentage += attackData.AttackDamage;


    	   //Calculate the direction for knockback
            float facingDir = Mathf.Sign(hitbox.Facing) == 0 ? 1 : Mathf.Sign(hitbox.Facing);

            // rotate direction by angle of impact
            //Do we still need all this math?
            //calculated angle


            float angle = Mathf.Sign(attackData.AngleOfImpact) * Mathf.Atan2(hurtbox.parent.transform.position.y-hitbox.parent.transform.position.y, hurtbox.parent.transform.position.x-hitbox.parent.transform.position.x)*180 / Mathf.PI;

            //KILL DI
            //float pro = drifter.input.MoveY < 0 ? 360f - Vector3.Angle(facingDir * Vector3.right,new Vector2(drifter.input.MoveX,drifter.input.MoveY)): Vector3.Angle(facingDir * Vector3.right,new Vector2(drifter.input.MoveX,drifter.input.MoveY));

            Vector3 adjustedAngle = Quaternion.Euler(0, 0, attackData.AngleOfImpact * facingDir)  * Vector2.right * facingDir;

            float horizontalComponent = facingDir * Mathf.Cos(attackData.AngleOfImpact *Mathf.Deg2Rad);
            float verticalComponent = Mathf.Sin(attackData.AngleOfImpact *Mathf.Deg2Rad);



            Vector2 forceDir = Mathf.Abs(attackData.AngleOfImpact) <= 360?
                                    adjustedAngle:
                                    Quaternion.Euler(0, 0, angle) * Vector2.right;


            //Calculate knockback magnitude
            float KB = GetKnockBack(percentage, 80, false, attackData);

            //Calculate hitstun duration
            HitstunDuration = GetHitStun(null, hitbox.parent.GetComponent<Drifter>(), attackData);


            //Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f);
            AttackFXSystem attackFX = attackData.HitFX;
            Vector2 hitSparkScale =  new Vector2(facingDir *10f, 10f);

            //apply attacker hitpause
            HitPauseDuration = (int)(HitstunDuration *.3f);
            if(attackData.HitStop >=0)
                HitPauseDuration += attackData.HitStop;


            if (hitbox.gameObject.tag != "Projectile")
                attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,(attackData.AttackDamage <=2f ? 1 : Mathf.Max(HitPauseDuration,2)));



            //Cause the screen to shake slightly on hit, as long as the move has knockback
            if(Shake != null && attackData.Knockback !=0){
                Shake.Shake((willCollideWithBlastZone(GetComponent<Rigidbody2D>(), HitstunDuration)?18:6),Mathf.Clamp((((attackData.Knockback - 10)/100f + (attackData.AttackDamage -10)/44f)) * attackData.KnockbackScale,.07f,.8f));
            }

            //If the defender is grounded, use the absolute value of the y component of the velocity
            //This prevents grounded opponents from getting stuck when spiked on the ground
            if(attackData.Knockback > 0 && attackData.AngleOfImpact > -361 && takesKnockback){
                rb.velocity = Vector3.zero;
                delayedVelocity = new Vector2(forceDir.normalized.x * KB, forceDir.normalized.y * KB);

            }

            //Autolink angle (<361) scales its magnitude with distacne from said point, further scaled with the attacker's velocity
            else if(attackData.Knockback > 0 && attackData.AngleOfImpact <= -361 && takesKnockback){
                rb.velocity = hitbox.parent.GetComponent<Rigidbody2D>().velocity * (1 + attackData.KnockbackScale);
            }

            float hitSparkAngle = facingDir * ((Mathf.Abs(attackData.AngleOfImpact) > 65f) ? Mathf.Sign(attackData.AngleOfImpact) * 90f : 0f);
            if (attackFX != null)
                attackFX.TriggerFXSystem(attackData.AttackDamage, HitstunDuration, hitSparkPos,attackData.AngleOfImpact * facingDir, adjustedAngle,  hitSparkScale);

            // Ancillary Hitsparks
            if (attackData.AttackDamage > 0f && attackFX != null)
                StartCoroutine(delayHitsparks(attackFX, hitSparkPos, attackData.AngleOfImpact, attackData.AttackDamage, HitstunDuration *.25f));

        }

    	return 2;

    }

    //Takes a snapshot of the current frame to rollback to
    public NPCHurtboxRollbackFrame SerializeFrame()
    {
        return new NPCHurtboxRollbackFrame()
        {
            HurtboxFrame = base.SerializeFrame(),
            TakesKnockback = takesKnockback,
            Percentage = percentage,
            Facing = facing,
            HitstunDuration = this.HitstunDuration,
            HitPauseDuration = this.HitstunDuration,
            DelayedVelocity = delayedVelocity,
            Hitboxes = GetComponent<InstantiatedEntityCleanup>().SerializeFrame()
        };
    }

    //Rolls back the entity to a given frame state
    public  void DeserializeFrame(NPCHurtboxRollbackFrame p_frame)
    {
            base.DeserializeFrame(p_frame.HurtboxFrame);
            takesKnockback = p_frame.TakesKnockback;
            percentage = p_frame.Percentage;
            facing =p_frame.Facing;
            HitstunDuration = p_frame.HitstunDuration;
            HitPauseDuration = p_frame.HitPauseDuration;
            delayedVelocity = p_frame.DelayedVelocity;
            GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(p_frame.Hitboxes);
    }
}

public class NPCHurtboxRollbackFrame: INetworkData
{
    public HurtboxRollbackFrame HurtboxFrame;

    public bool TakesKnockback;
    public float Percentage;  
    public int HitstunDuration;
    public int HitPauseDuration;
    public Vector3 DelayedVelocity;
    public int Facing;
    public BasicProjectileRollbackFrame Hitboxes;

    public string Type { get; set; }

}