using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonplayerHurtboxHandler : PlayerHurtboxHandler
{

	public float percentage = 0f;
    protected float HitstunDuration = 0;

	public override int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, DrifterAttackType attackType, SingleAttackData attackData)
    {

        if (GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && hurtbox.owner != hitbox.parent && !oldAttacks.ContainsKey(attackID))
        {
            
            // register new attack
            oldAttacks[attackID] = Time.time;

    	    percentage += attackData.AttackDamage;


    	    PlayerStatus attackerStatus = hitbox.parent.GetComponent<PlayerStatus>();

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
            float KB = (float)((percentage / 10f + percentage * attackData.AttackDamage / 20f)
                        * 200 / (180f) * 1.4
                        *  attackData.KnockbackScale + attackData.Knockback);

            //Calculate hitstun duration
            HitstunDuration = (attackData.HitStun>=0 || attackData.hasStaticHitstun)?attackData.HitStun * framerateScalar:(KB*.006f + .1f);


            Vector3 hitSparkPos = hurtbox.capsule.ClosestPoint(hitbox.parent.transform.position);

            //Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f);
            AttackFXSystem attackFX = attackData.HitFX;
            Vector2 hitSparkScale =  new Vector2(facingDir *10f, 10f);


            //Cause the screen to shake slightly on hit, as long as the move has knockback
            if(Shake != null && attackData.Knockback !=0){
                Shake.startShakeCoroutine((willCollideWithBlastZone(GetComponent<Rigidbody2D>(), HitstunDuration)?0.3f:0.1f),Mathf.Clamp((((attackData.Knockback - 10)/100f + (attackData.AttackDamage -10)/44f)) * attackData.KnockbackScale,.07f,.8f));
            }

            //If the defender is grounded, use the absolute value of the y component of the velocity
            //This prevents grounded opponents from getting stuck when spiked on the ground
            if(attackData.Knockback > 0 && attackData.AngleOfImpact > -361){
                GetComponent<Rigidbody2D>().velocity = new Vector2(forceDir.normalized.x * KB, forceDir.normalized.y * KB);

            }

            //Autolink angle (<361) scales its magnitude with distacne from said point, further scaled with the attacker's velocity
            else if(attackData.Knockback > 0 && attackData.AngleOfImpact <= -361){
                GetComponent<Rigidbody2D>().velocity = hitbox.parent.GetComponent<Rigidbody2D>().velocity * (1 + attackData.KnockbackScale);
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


}