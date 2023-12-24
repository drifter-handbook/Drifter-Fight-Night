using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerHurtboxHandler : MonoBehaviour
{
	// keep track of what attacks we've already processed
	// AttackID -> Timestamp
	public int[] oldAttacks = new int[128];
	public int framesSinceCleaned = 0; 


	protected const int MAX_ATTACK_DURATION = 240;

	// for creating hitsparks

	protected ScreenShake Shake;

	[NonSerialized]
	public TrainingUIManager trainingUI;

	// Start is called before the first frame update
	protected void Start()	{

		Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();
		//entity = GetComponent<InstantiatedEntityCleanup>();
		// StartCoroutine(CleanupOldAttacks());
	}

	public bool CanHit(int attackID)	{
		CleanupOldAttacks();
		return oldAttacks[attackID] == 0;
	}

	void CleanupOldAttacks()	{
		for(int i = 0; i < oldAttacks.Length; i++) {
			if(oldAttacks[i] != 0 ) oldAttacks[i] = Mathf.Max(0,oldAttacks[i] - framesSinceCleaned);
		}
		framesSinceCleaned = 0;
	}

	public virtual void UpdateFrame() {
		framesSinceCleaned++;
		if(framesSinceCleaned > MAX_ATTACK_DURATION)CleanupOldAttacks(); 
	}

	//Registers an attack hit. Returns an integer based on what happened.
	// -5: hit registered againat an invulnerable enemy
	// -4: Hit was a registered as a counter
	// -3: Hit did not register at all; ID was already present in dict, or target was invulnerable.
	// -2: Hit was registerted, but Parried, dealing no damage
	// -1: Hit was registered, but blocked
	// 0: Hit was registered normally
	// 1: Hit was registered normally and has attatched the opponent to the players hitbox
	// 2: hit was against a non-player object

	public virtual int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID,  SingleAttackData attackData)	{
		int returnCode = -3;

		if (hitbox.parent != hurtbox.parent && CanHit(attackID)) {
			// register new attack
			Drifter drifter = GetComponent<Drifter>();
			PlayerStatus status = drifter.status;

			Drifter attacker = hitbox.parent.GetComponent<Drifter>();
			PlayerStatus attackerStatus = attacker.status;

			float damageDealt = 0f;


			//Whiff on based on state and hit type
			if(
				//Whiff air only moves against grounded opponenets
				(!attackData.canHitGrounded && drifter.movement.grounded) ||
				//Whiff ground only moves on aerial opponenets
				(!attackData.canHitAerial && !drifter.movement.grounded) ||
				//Whiff grabs and command grabs on jumping opponents
				((drifter.movement.dashing || status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN)) && attackData.hitType == HitType.GRAB ) || 
				//Whiff non-OTG moves on otg opponents
				(!attackData.canHitKnockedDown && status.HasStatusEffect(PlayerStatusEffect.FLATTEN))
			) return -3;

			if(status.HasStatusEffect(PlayerStatusEffect.INVULN)) return -3;
			oldAttacks[attackID] = MAX_ATTACK_DURATION;

			if((drifter.guarding && status.HasStunEffect()) &&  attackData.hitType == HitType.GRAB) return -1;

			//Ignore the collision if invulnerable or You try to grab a planted opponenet
			
			if(status.HasStatusEffect(PlayerStatusEffect.PLANTED) && attackData.StatusEffect == PlayerStatusEffect.GRABBED) return -3;

			attacker.canFeint = false;

			Vector3 hitSparkPos = hurtbox.capsule.ClosestPoint(hitbox.parent.transform.position);
			
			//Freezefame if hit a counter
			if(hurtbox.gameObject.name == "Counter" &&  attackData.AttackDamage >0f && attackData.hitType!=HitType.GRAB) {
				Shake?.Darken(25);
				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.STAR, hitSparkPos,0, new Vector2(10f, 10f));
				attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,30);
				drifter.PlayAnimation("Counter_Success");
				status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,15);
				return -4;
			}


			bool crossUp = (hitbox.parent.transform.localPosition.x > transform.localPosition.x  && drifter.movement.Facing < 0) 
						|| (hitbox.parent.transform.localPosition.x < transform.localPosition.x  && drifter.movement.Facing > 0 && attackData.AttackDamage > 0f);

			// apply damage
			if (drifter != null && status != null) {
				damageDealt = 
					//Base Damage + flat damage increases
					(attackData.AttackDamage + (status.HasStatusEffect(PlayerStatusEffect.DEFENSEDOWN) &&  attackData.AttackDamage >0 ? 1.7f : 0f))

					//Blocking damage Reduction
					//0 chip damage on perfect guard
					  * ((drifter.guarding && attackData.hitType!=HitType.GRAB && !crossUp) ? (drifter.perfectGuarding || drifter.parrying? 0 : .2f): 1f)

					//Defense Buff damage reduction
					  * (status.HasStatusEffect(PlayerStatusEffect.DEFENSEUP) ? 0.7f:1f)

					//Attacker damage buff)
					  * (attackerStatus.HasStatusEffect(PlayerStatusEffect.DAMAGEUP)?1.5f:1f);

				drifter.DamageTaken += damageDealt;

			}


			//Calculate the direction for knockback
			float facingDir = attackData.mirrorKnockback? (hurtbox.capsule.bounds.center.x > hitbox.gameObject.GetComponent<Collider2D>().bounds.center.x ? 1: -1) : Mathf.Sign(hitbox.Facing) == 0 ? 1 : Mathf.Sign(hitbox.Facing);

			

			// rotate direction by angle of impact
			//Do we still need all this math?
			//calculated angle


			float angle = Mathf.Sign(attackData.AngleOfImpact) * Mathf.Atan2(hurtbox.parent.transform.position.y-hitbox.parent.transform.position.y, hurtbox.parent.transform.position.x-hitbox.parent.transform.position.x)*180 / Mathf.PI;

			//KILL DI
			//float directionInfluenceAngle = drifter.input[0].MoveY < 0 ? 360f - Vector3.Angle(facingDir * Vector3.right,new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY)): Vector3.Angle(facingDir * Vector3.right,new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY));


			Vector3 adjustedAngle = Quaternion.Euler(0, 0, attackData.AngleOfImpact * facingDir)  * Vector2.right * facingDir;

			float horizontalComponent = facingDir * Mathf.Cos(attackData.AngleOfImpact *Mathf.Deg2Rad);
			float verticalComponent = Mathf.Sin(attackData.AngleOfImpact *Mathf.Deg2Rad);

			//DI Angle Adjustment
			if(drifter.input[0].MoveX !=0 || drifter.input[0].MoveY !=0 )
				adjustedAngle = Quaternion.Euler(0, 0, 
					Mathf.Atan(
							(verticalComponent + .2f * drifter.input[0].MoveY)/
							(horizontalComponent+ .2f * drifter.input[0].MoveX)
						)
						* Mathf.Rad2Deg) 
						* Vector2.right * Mathf.Sign(horizontalComponent);
						
			//Autolink angle (<-361) sets the knockback angle to send towards the hitbox's centerpoint
			Vector2 forceDir = Mathf.Abs(attackData.AngleOfImpact) <= 360?
									adjustedAngle:
									Quaternion.Euler(0, 0, angle) * Vector2.right;


			//Calculate knockback magnitude
			float KB = GetKnockBack(drifter.DamageTaken, drifter.movement.Weight, 
									(status.HasStatusEffect(PlayerStatusEffect.EXPOSED) || status.HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT)),
									attackData);


			//COMBO DI
			if(KB < 25 && (drifter.input[0].MoveX !=0 || drifter.input[0].MoveY !=0 )) {
					
				if(Mathf.Abs(horizontalComponent) >= Mathf.Abs(verticalComponent)) KB *= horizontalComponent * drifter.input[0].MoveX < 0 ? .4f:  1.4f;
				else KB *= verticalComponent * drifter.input[0].MoveY < 0 ? .4f:  1.4f;

			}

			//Calculate hitstun duration
			int HitstunDuration = GetHitStun(drifter, attacker, attackData);
			int HitPauseDuration = attackData.HitStop >=0 ? attackData.HitStop : HitstunDuration;

			//damage numbers managment
			
			status?.ApplyDamage(damageDealt, HitstunDuration);

			//Flags a guradbreak for BIGG HITSPARKS
			bool guardbroken = false;


			AttackFXSystem attackFX = attackData.HitFX;
			if (attackFX == null) {
				Debug.LogWarning("Attack is missing an AttackFXSystem. You should probably fix that.");
			}
			Vector2 hitSparkScale =  new Vector2(facingDir *10f, 10f);
			bool isCritical = false;
			bool isBlocked = false;
			bool hadSlowmo = status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION);

			//Ignore knockback if invincible or armoured
			if(attackData.hitType == HitType.TRANSCENDANT) {
				status?.ApplyStatusEffect(attackData.StatusEffect,attackData.StatusDuration);
				returnCode = 0;
			}
			else if (status != null && (attackData.hitType==HitType.GRAB || !drifter.guarding || crossUp) && !drifter.parrying){

				drifter.knockedDown = false;
				//drifter.clearGuardFlags();

				if((attackData.hitType==HitType.GRAB || crossUp) && drifter.guarding && attackData.AttackDamage >0f) {
					//status.ApplyStatusEffect(PlayerStatusEffect.GUARDBROKEN,5f);
					HitstunDuration = 60;
					guardbroken = true;
					drifter.clearGuardFlags();
					
				}
				//else drifter.guardBreaking = false;

				//As long as the defender isnt in superarmour, or they are being grabbed, apply knockback velocity
				if(!status.HasStatusEffect(PlayerStatusEffect.ARMOUR) || attackData.hitType==HitType.GRAB || (crossUp && drifter.guarding) ){

					status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0);
					
					if(damageDealt >0)drifter.movement.setFacing((int)(facingDir *-1) * ((attackData.AngleOfImpact > 90 && attackData.AngleOfImpact <= 270)?-1:1));

					//Cause the screen to shake slightly on hit, as long as the move has knockback
					if(Shake != null && attackData.Knockback !=0){
						Shake.Shake((willCollideWithBlastZone(GetComponent<Rigidbody2D>(), HitstunDuration)?18:9),Mathf.Clamp((((attackData.Knockback - 10)/100f + (damageDealt-10)/44f)) * attackData.KnockbackScale,.25f,.8f));

						//Shake.startShakeCoroutine(.3f,2f);
					}

					//If the defender is grounded, use the absolute value of the y component of the velocity
					//This prevents grounded opponents from getting stuck when spiked on the ground
					if(attackData.Knockback > 0 && attackData.AngleOfImpact > -361){
						GetComponent<Rigidbody2D>().velocity = new Vector2(forceDir.normalized.x * KB, drifter.movement.grounded?Mathf.Abs(forceDir.normalized.y * KB): forceDir.normalized.y * KB);
						
						//Use restitution particle if spiked on the grounde
						if(drifter.movement.grounded && attackData.AngleOfImpact < 285 &&  attackData.AngleOfImpact > 255)drifter.movement.spawnJuiceParticle(transform.position + new Vector3(0,-2.5f,0), MovementParticleMode.Restitution);
					}

					//Autolink angle (<361) scales its magnitude with distacne from said point, further scaled with the attacker's velocity
					else if(attackData.Knockback > 0 && attackData.AngleOfImpact <= -361){
						GetComponent<Rigidbody2D>().velocity = hitbox.parent.GetComponent<Rigidbody2D>().velocity * (1 + attackData.KnockbackScale);
					}
										
					//IF there is hitstun to be applied, apply it
					if(HitstunDuration > 0)	{
						//Apply a minimum hitstun on burst type attacks
						if(attackData.hitType != HitType.BURST || HitstunDuration >= status?.remainingDuration(PlayerStatusEffect.KNOCKBACK))
							status?.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK, HitstunDuration);
										
					}
				}

				if(attackData.StatusEffect != PlayerStatusEffect.PLANTED || drifter.movement.grounded){


					if(attackData.StatusEffect == PlayerStatusEffect.PLANTED && !status.HasStatusEffect(PlayerStatusEffect.PLANTED)) GetComponent<Rigidbody2D>().velocity = Vector3.down*5f;
					status.ApplyStatusEffect(attackData.StatusEffect, attackData.StatusDuration);

					//Attatch Defender to attacker's hitbox for grab moves.
					if(attackData.StatusEffect == PlayerStatusEffect.GRABBED)	{
						status.grabPoint = hitbox.gameObject.GetComponent<Collider2D>();
						attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,5);

					}               
				}
				//Pop players out of the ground when they are already grounded
				else if(attackData.StatusEffect == PlayerStatusEffect.PLANTED && !drifter.movement.grounded)
					status.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,30);


				//Extend hitpause on kill
				if (willCollideWithBlastZoneAccurate(GetComponent<Rigidbody2D>(), HitstunDuration) && drifter.Stocks <= 1 && NetworkPlayers.Instance.players.Values.Where(x => x != null).ToList().Count <=2) {
					HitstunDuration = 180;
					//drifter.movement.techWindowElapsed = 2f;
				} 
				else if (willCollideWithBlastZone(GetComponent<Rigidbody2D>() , HitstunDuration) ) Mathf.Min(HitstunDuration*=2,3f);
				
				
				if(status.HasStatusEffect(PlayerStatusEffect.ARMOUR) && attackData.hitType!=HitType.GRAB)
					Shake?.Darken(25);

				
				//If hitstop is scaled, and one is proviced, sum the hitstun duuration and the hitpause duration
				 
				HitPauseDuration = (guardbroken || status.HasStatusEffect(PlayerStatusEffect.ARMOUR)) ? 30 : HitPauseDuration;

				//Apply defender hitpause
				if(HitPauseDuration >0 && attackData.StatusEffect != PlayerStatusEffect.HITPAUSE )
					status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE, HitPauseDuration * (hadSlowmo?2:1));
				
				//apply attacker hitpause

				if(HitPauseDuration >0) {
					if(hitbox.gameObject.tag != "Projectile")
						attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,HitPauseDuration);
					else
						hitbox.gameObject.GetComponentInParent<InstantiatedEntityCleanup>()?.ApplyFreeze(HitPauseDuration);
				}

				drifter.GetComponentInChildren<GameObjectShake>().Shake(attackData.StatusEffect != PlayerStatusEffect.CRINGE?attackData.HitStop:attackData.StatusDuration,attackData.StatusEffect != PlayerStatusEffect.CRINGE?1.5f:2f);

				returnCode = attackData.StatusEffect == PlayerStatusEffect.GRABBED?1: 0;             
			}
			//Normal guarding behavior
			else if(drifter.guarding && !drifter.parrying) {

				drifter.movement.updateFacing();
				//if(attackData.hitType==HitType.GUARD_CRUSH)drifter.guardBreaking = true;
				//push both players back on guarrd
				
				if(hitbox.gameObject.tag != "Projectile")
					 hitbox.parent.GetComponent<Rigidbody2D>().velocity = new Vector2(-Mathf.Sign(forceDir.normalized.x) * attackData.pushBlock, hitbox.parent.GetComponent<Rigidbody2D>().velocity.y);

				//hitbox.parent.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Clamp(HitstunDuration,.2f,1f) * hitbox.Facing *-15f, hitbox.parent.GetComponent<Rigidbody2D>().velocity.y);
			   
				//No pushback on perfect guard
				if(!drifter.perfectGuarding) {
					// Get new particle for prefect guarda
					GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign(forceDir.normalized.x) * attackData.pushBlock, GetComponent<Rigidbody2D>().velocity.y);

				}

				else drifter.movement.spawnJuiceParticle(hitSparkPos, MovementParticleMode.Parry);
				//put defender in blockstun
				if(HitstunDuration > 0){
						// status?.calculateFrameAdvantage(HitstunDuration, attacker.getRemainingAttackTime());
						//6x blockstun on guardcrush
						status?.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK, HitstunDuration);
						//status?.ApplyStatusEffect(PlayerStatusEffect.GUARDCRUSHED, HitstunDuration);
				}

				 //Apply defender hitpause
				if(HitPauseDuration >0 && attackData.StatusEffect != PlayerStatusEffect.HITPAUSE )
					status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE, HitPauseDuration * (hadSlowmo?2:1));
				
				//apply attacker hitpause
				if (HitPauseDuration >0 && hitbox.gameObject.tag != "Projectile")
					attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,HitPauseDuration);

				returnCode = -1; 

			}
			//Parrying a guardbreaker
			else if(drifter.parrying && attackData.hitType==HitType.GRAB && hitbox.gameObject.tag != "Projectile") {

				//STODO Shit out lots of particles here
				if(hitbox.gameObject.tag != "Projectile")hitbox.parent.GetComponent<Rigidbody2D>().velocity = new Vector2(hitbox.Facing *-35f, hitbox.parent.GetComponent<Rigidbody2D>().velocity.y);
			   
				GetComponent<Rigidbody2D>().velocity = new Vector2(35f * hitbox.Facing , GetComponent<Rigidbody2D>().velocity.y);
				returnCode = -2;

			}
			//Parrying a normal attack
			else if(drifter.parrying && hitbox.gameObject.tag != "Projectile") {
				//TODO Shit out more paricles
				//TODO Make this work better in air

				drifter.movement.spawnJuiceParticle(hitSparkPos, MovementParticleMode.Parry);

				attackerStatus.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,60);
				attackerStatus.ApplyStatusEffect(PlayerStatusEffect.CRINGE,60);
				Shake.zoomEffect(36,Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),false);
				attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,36);
				drifter.movement.pauseGravity();
				returnCode = -2;

			}

			trainingUI?.readFrameAdvantage(attackerStatus,status);
			
			// create hit sparks
			

			//When Guardbroken, play the crit animation
			isCritical = guardbroken || crossUp;
			isBlocked = drifter != null && drifter.guarding && attackData.hitType!=HitType.GRAB && !isCritical && attackData.AttackDamage >0f;

			float hitSparkAngle = attackData.AngleOfImpact;
			
			if (attackFX != null && !isBlocked)
				attackFX.TriggerFXSystem(attackData.AttackDamage, HitstunDuration, hitSparkPos, attackData.AngleOfImpact * facingDir, adjustedAngle, hitSparkScale);
			
			if (isBlocked)
				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.GUARD_STRONG, hitSparkPos, 0, -1 * hitSparkScale);
			else if (isCritical)
				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.CRIT, hitSparkPos, hitSparkAngle, hitSparkScale);

			if (drifter != null && willCollideWithBlastZone(GetComponent<Rigidbody2D>(), HitstunDuration)) {
				//hitSparkPos = Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f);
				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.CRIT, hitSparkPos, 0, new Vector2(facingDir * 10f, 10f));

				if(drifter.Stocks <= 1 && willCollideWithBlastZoneAccurate(GetComponent<Rigidbody2D>(), HitstunDuration))
					Shake.zoomEffect(30,hitSparkPos,true);
			}

			if (isCritical)
				Shake.Darken(30);

			// Ancillary Hitsparks
			if (drifter != null && damageDealt >0f && attackFX != null)
				StartCoroutine(delayHitsparks(attackFX, hitSparkPos, attackData.AngleOfImpact, damageDealt, HitstunDuration *.25f));
		

			//METER BUILD
				// -4: Hit was a registered as a counter
				// -3: Hit did not register at all; ID was already present in dict, or target was invulnerable.
				// -2: Hit was registerted, but Parried, dealing no damage
				// -1: Hit was registered, but blocked
				// 0: Hit was registered normally
				// 1: hit was against a non-player object
			switch(returnCode) {
				case 1:
					attacker.gainSuperMeter(.04f);
					break;
				case 0:
					attacker.gainSuperMeter(damageDealt *.02f);
					drifter.gainSuperMeter(.05f);
					break;
				case -1:
					attacker.gainSuperMeter(.06f);
					drifter.gainSuperMeter(.06f);
					break;
				case -2:
					drifter.gainSuperMeter(.5f);
					break;
				case -4:
					drifter.gainSuperMeter(.33f);
					break;
				default:
					break;
			}


		}

		return returnCode;
	}

	protected IEnumerator delayHitsparks(AttackFXSystem attackFX, Vector3 position, float angle,float damage, float p_duration)	{
		float duration = p_duration/60f;
		Vector3 hitSparkPos = position;
		float angleT;
		float stepSize = duration / ((damage + 2 )/3);
		
		for (int i = 0; i < (damage + 2 )/3 ; i++) {
			angleT = angle + UnityEngine.Random.Range(-45, 45);
			hitSparkPos += Quaternion.Euler(0, 0, angleT) * new Vector3(-UnityEngine.Random.Range(1, 4), 0, 0);
			GraphicalEffectManager.Instance.CreateHitSparks(attackFX.GetSpark(), position, angleT, new Vector2(10f, 10f));

			angleT += 180;

			hitSparkPos += Quaternion.Euler(0, 0, angleT) * new Vector3(-UnityEngine.Random.Range(1, 4), 0, 0);
			GraphicalEffectManager.Instance.CreateHitSparks(attackFX.GetSpark(), hitSparkPos, angleT, new Vector2(10f, 10f));

			yield return new WaitForSeconds(stepSize);
		}
		
		yield break;
	}

	protected float GetKnockBack(float damageTaken, float weight, bool strong, SingleAttackData attackData) {
		
		float effectiveDamage = damageTaken;
		float effectiveCeiling = attackData.scalingUpperBound;

		//Sets the effective ceiling if both bounds are used.
		if(attackData.scalingUpperBound >= 0 &&  attackData.scalingLowerBound >= 0) effectiveCeiling = (attackData.scalingUpperBound - attackData.scalingLowerBound);

		//If the drifter is below the damage floor, no scaling. If there is a floor and they are above it, subtract the floor from their damage and use that
		if(attackData.scalingLowerBound >= 0 && damageTaken < attackData.scalingLowerBound) effectiveDamage = Mathf.Max(0,damageTaken - attackData.scalingLowerBound);

		//if there is a ceiling, and the drifter is above that, set their effective damage to the ceiling, minus the floor, if there is one.
		if(attackData.scalingUpperBound >= 0 && damageTaken > attackData.scalingUpperBound) effectiveDamage = effectiveCeiling;


		return (float)(((effectiveDamage * 125f) / (weight + 100f) *
						 (strong?1.5f:1)) * attackData.KnockbackScale + attackData.Knockback);
	}

	protected int GetHitStun(Drifter defender, Drifter attacker, SingleAttackData attackData) {
		int adv = attackData.HitStun;
		if (defender != null && defender.guarding)
			adv = attackData.ShieldStun;
		
		if (attackData.dynamicStun)
			adv += attackData.finalFrame - attackData.firstActiveFrame;

		return adv;
	}

	protected bool willCollideWithBlastZone(Rigidbody2D rigidbody, int hitstun)	{
		float xDel, yDel;
		float xVel, yVel;

		GameObject parentZone = GameObject.Find("Kill Zones");
		Transform hZone, vZone;
		if (rigidbody.velocity.x > 0)
			hZone = parentZone.transform.Find("Killzone Right");
		else
			hZone = parentZone.transform.Find("Killzone Left");

		if (rigidbody.velocity.y > 0)
			vZone = parentZone.transform.Find("Killzone Top");
		else
			vZone = parentZone.transform.Find("Killzone Bottom");

		xDel = Mathf.Abs(hZone.position.x - rigidbody.position.x);
		yDel = Mathf.Abs(vZone.position.y - rigidbody.position.y);
		xVel = Mathf.Abs(rigidbody.velocity.x);
		yVel = Mathf.Abs(rigidbody.velocity.y);



		if (xVel * hitstun/60f >= xDel || yVel * hitstun/60f >= yDel)
			return true;

		return false;
	}

	protected bool willCollideWithBlastZoneAccurate(Rigidbody2D rigidbody, int hitstun)	{
		float xDel, yDel;
		float xVel, yVel;

		GameObject parentZone = GameObject.Find("Kill Zones");
		Transform hZone, vZone;
		if (rigidbody.velocity.x > 0)
			hZone = parentZone.transform.Find("Killzone Right");
		else
			hZone = parentZone.transform.Find("Killzone Left");

		if (rigidbody.velocity.y > 0)
			vZone = parentZone.transform.Find("Killzone Top");
		else
			vZone = parentZone.transform.Find("Killzone Bottom");

		xDel = 1.2f * Mathf.Abs(hZone.position.x - rigidbody.position.x);
		yDel = 1.2f * Mathf.Abs(vZone.position.y - rigidbody.position.y);
		xVel = 1.2f * Mathf.Abs(rigidbody.velocity.x);
		yVel = 1.2f * Mathf.Abs(rigidbody.velocity.y);

		float g = rigidbody.gravityScale * Physics2D.gravity.y;

		// if (Mathf.Sign(rigidbody.velocity.y) == -1)
		// {
		//     g *= -1;
		// }

		//Dont play kill if youre gonna hit the stage
		if(rigidbody.velocity.y < 0) {
			 RaycastHit2D[] hits = new RaycastHit2D[10];

			int count = Physics2D.RaycastNonAlloc(rigidbody.position, Vector2.down, hits,rigidbody.velocity.y );

			for (int i = 0; i < count; i++) if (hits[i].collider.gameObject.tag == "Ground" || (hits[i].collider.gameObject.tag == "Platform")) return false;
		}
	   

		if (xVel * hitstun/60f >= xDel || yVel * hitstun + (0.5 * g * hitstun * hitstun/60f) >= yDel)
			return true;

		return false;
	}

	//Takes a snapshot of the current frame to rollback to
	public HurtboxRollbackFrame SerializeFrame()	{
		int[] p_Attacks = new int[128];

		Array.Copy(oldAttacks,p_Attacks,128);

		return new HurtboxRollbackFrame() {
			OldAttacks = p_Attacks,
			FramesSinceCleaned = framesSinceCleaned,
		};
	}

	//Rolls back the entity to a given frame state
	public  void DeserializeFrame(HurtboxRollbackFrame p_frame)	{
			Array.Copy(p_frame.OldAttacks,oldAttacks,128);
			framesSinceCleaned = p_frame.FramesSinceCleaned;
	}
}

public class HurtboxRollbackFrame: INetworkData
{
	public string Type { get; set; }
	public int[] OldAttacks;
	public int FramesSinceCleaned;

}
