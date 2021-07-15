using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerHurtboxHandler : MonoBehaviour
{
    // keep track of what attacks we've already processed
    // AttackID -> Timestamp
    public Dictionary<int, float> oldAttacks = new Dictionary<int, float>();
    const float MAX_ATTACK_DURATION = 7f;

    // for creating hitsparks
    protected NetworkHost host;
    protected ScreenShake Shake;

    static protected float framerateScalar =.0833333333f;

    // Start is called before the first frame update
    protected void Start()
    {
        host = GameController.Instance.host;
        Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();

        StartCoroutine(CleanupOldAttacks());
    }

    public bool CanHit(int attackID)
    {
        return !oldAttacks.ContainsKey(attackID);
    }


    //Registers an attack hit. Returns an integer based on what happened.
    // -4: Hit was a registered as a counter
    // -3: Hit did not register at all; ID was already present in dict, or target was invulnerable.
    // -2: Hit was registerted, but Parried, dealing no damage
    // -1: Hit was registered, but blocked
    // 0: Hit was registered normally
    // 1: hit was against a non-player object

    public virtual int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, DrifterAttackType attackType, SingleAttackData attackData)
    {
        //UnityEngine.Debug.Log(attackID);
        // only host processes hits, don't hit ourself, and ignore previously registered attacks
        int returnCode = -3;
        if (GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && !oldAttacks.ContainsKey(attackID))
        {
            
            // register new attack
            oldAttacks[attackID] = Time.time;
            // apply hit effects
            hitbox.parent.GetComponent<PlayerAttacks>().Hit(attackType, attackID, hurtbox.parent);

            Drifter drifter = GetComponent<Drifter>();

            PlayerStatus status = drifter.status;
            PlayerStatus attackerStatus = hitbox.parent.GetComponent<PlayerStatus>();

            float damageDealt = 0f;

            //Ignore the collision if invulnerable
            if(status.HasStatusEffect(PlayerStatusEffect.INVULN))return -3;
            
            //Freezefame if hit a counter
            if(hurtbox.gameObject.name == "Counter" &&  attackData.AttackDamage >0f && !attackData.isGrab)
            {
                GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.STAR, Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),0, new Vector2(10f, 10f));
                attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.7f);
                status.ApplyStatusEffect(PlayerStatusEffect.HIT,.3f);
                return -4;
            }

            // apply damage
            if (drifter != null && status != null)
            {
                damageDealt = 
                    //Base Damage + flat damage increases
                    (attackData.AttackDamage + (status.HasStatusEffect(PlayerStatusEffect.DEFENSEDOWN) &&  attackData.AttackDamage >0 ? 1.7f : 0f))

                    //Blocking damage Reduction
                    //0 chip damage on perfect guard
                      * (drifter.guarding && !attackData.isGrab ? (drifter.perfectGuarding || drifter.parrying? 0 : 1 - drifter.BlockReduction): 1f)

                    //Defense Buff damage reduction
                      * (status.HasStatusEffect(PlayerStatusEffect.DEFENSEUP) ? 0.7f:1f)

                    //Attacker damage buff)
                      * (attackerStatus.HasStatusEffect(PlayerStatusEffect.DAMAGEUP)?1.5f:1f);

                drifter.DamageTaken += damageDealt;

            }


            //Calculate the direction for knockback
            float facingDir = Mathf.Sign(hitbox.Facing) == 0 ? 1 : Mathf.Sign(hitbox.Facing);

            // rotate direction by angle of impact
            //Do we still need all this math?
            //calculated angle


            float angle = Mathf.Sign(attackData.AngleOfImpact) * Mathf.Atan2(hurtbox.parent.transform.position.y-hitbox.parent.transform.position.y, hurtbox.parent.transform.position.x-hitbox.parent.transform.position.x)*180 / Mathf.PI;

            //KILL DI
            float directionInfluenceAngle = drifter.input.MoveY < 0 ? 360f - Vector3.Angle(facingDir * Vector3.right,new Vector2(drifter.input.MoveX,drifter.input.MoveY)): Vector3.Angle(facingDir * Vector3.right,new Vector2(drifter.input.MoveX,drifter.input.MoveY));

            Vector3 adjustedAngle = Quaternion.Euler(0, 0, attackData.AngleOfImpact * facingDir)  * Vector2.right * facingDir;

            float horizontalComponent = facingDir * Mathf.Cos(attackData.AngleOfImpact *Mathf.Deg2Rad);
            float verticalComponent = Mathf.Sin(attackData.AngleOfImpact *Mathf.Deg2Rad);


            //DI Angle Adjustment
            if(drifter.input.MoveX !=0 || drifter.input.MoveY !=0 )adjustedAngle = Quaternion.Euler(0, 0, Mathf.Atan((verticalComponent * 1 + .2f * drifter.input.MoveY)/(horizontalComponent* 1 + .2f * drifter.input.MoveX)) * Mathf.Rad2Deg)  * Vector2.right * facingDir;

            //Autolink angle (<-361) sets the knockback angle to send towards the hitbox's centerpoint
            Vector2 forceDir = Mathf.Abs(attackData.AngleOfImpact) <= 360?
                                    adjustedAngle:
                                    Quaternion.Euler(0, 0, angle) * Vector2.right;


            //Calculate knockback magnitude
            float KB = (float)(((drifter.DamageTaken / 10f + drifter.DamageTaken * damageDealt / 20f)
                        * 200 / (GetComponent<PlayerMovement>().Weight + 100) * 1.4 *
                         ((status.HasStatusEffect(PlayerStatusEffect.EXPOSED) || status.HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))
                            ?1.5f:1)) * attackData.KnockbackScale + attackData.Knockback);


            //COMBO DI
            if(KB < 25 && (drifter.input.MoveX !=0 || drifter.input.MoveY !=0 ))
            {
                    
                if(Mathf.Abs(horizontalComponent) >= Mathf.Abs(verticalComponent)) KB *= horizontalComponent * drifter.input.MoveX < 0 ? .4f:  1.4f;
                else KB *= verticalComponent * drifter.input.MoveY < 0 ? .4f:  1.4f;

            }

            //Calculate hitstun duration
            float HitstunDuration = (attackData.HitStun>=0 || attackData.hasStaticHitstun)?attackData.HitStun * framerateScalar:(KB*.006f + .1f);

            //damage numbers managment
                if (status != null)
                    status.ApplyDamage(damageDealt, status.isInCombo, HitstunDuration);

            //Flags a guradbreak for BIGG HITSPARKS
            bool guardbroken = false;


            Vector3 hitSparkPos = hurtbox.capsule.ClosestPoint(hitbox.parent.transform.position);
            //Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f);
            HitSpark hitSparkMode = HitSpark.POKE;
            Vector2 hitSparkScale =  new Vector2(facingDir *10f, 10f);

            //Ignore knockback if invincible or armoured
            if (status != null && (attackData.isGrab || !drifter.guarding) && !drifter.parrying){

                //If the player treid to guard a guardbreaker, they loose their shield for 5 seconds (60 frames)
                if(attackData.isGrab && drifter.guarding)
                {
                    status.ApplyStatusEffect(PlayerStatusEffect.GUARDBROKEN,5f);
                    HitstunDuration = 1f;
                    guardbroken = true;
                    drifter.clearGuardFlags();
                    drifter.guardBreaking = true;
                    status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.6f);
                    
                }
                else drifter.guardBreaking = false;

                //As long as the defender isnt in superarmour, or they are being grabbed, apply knockback velocity
                if(!status.HasStatusEffect(PlayerStatusEffect.ARMOUR) || attackData.isGrab){

                    status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0f);

                    //Cause the screen to shake slightly on hit, as long as the move has knockback
                    if(Shake != null && attackData.Knockback !=0){
                        Shake.startShakeCoroutine((willCollideWithBlastZone(GetComponent<Rigidbody2D>(), HitstunDuration)?0.3f:0.1f),Mathf.Clamp((((attackData.Knockback - 10)/100f + (damageDealt-10)/44f)) * attackData.KnockbackScale,.07f,.8f));
                    }

                    //If the defender is grounded, use the absolute value of the y component of the velocity
                    //This prevents grounded opponents from getting stuck when spiked on the ground
                    if(attackData.Knockback > 0 && attackData.AngleOfImpact > -361){
                        GetComponent<Rigidbody2D>().velocity = new Vector2(forceDir.normalized.x * KB, GetComponent<PlayerMovement>().grounded?Mathf.Abs(forceDir.normalized.y * KB): forceDir.normalized.y * KB);

                        //Use restitution particle if spiked on the grounde
                        if(GetComponent<PlayerMovement>().grounded && attackData.AngleOfImpact < -75 &&  attackData.AngleOfImpact > -105)GetComponent<PlayerMovement>().spawnJuiceParticle(transform.position + new Vector3(0,-2.5f,0), MovementParticleMode.Restitution);
                    }

                    //Autolink angle (<361) scales its magnitude with distacne from said point, further scaled with the attacker's velocity
                    else if(attackData.Knockback > 0 && attackData.AngleOfImpact <= -361){
                        GetComponent<Rigidbody2D>().velocity = hitbox.parent.GetComponent<Rigidbody2D>().velocity * (1 + attackData.KnockbackScale);
                    }
                                        
                    //IF there is hitstun to be applied, apply it
                    if(attackData.HitStun != 0)
                    {
                        status?.calculateFrameAdvantage(HitstunDuration,hitbox.parent.GetComponent<Drifter>().getRemainingAttackTime());
                        status?.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK, HitstunDuration);
                    }
                }

                if(attackData.StatusEffect != PlayerStatusEffect.PLANTED || GetComponent<PlayerMovement>().grounded){

                    status?.calculateFrameAdvantage(attackData.StatusDuration* framerateScalar,hitbox.parent.GetComponent<Drifter>().getRemainingAttackTime());

                	if(attackData.StatusEffect == PlayerStatusEffect.PLANTED && !status.HasStatusEffect(PlayerStatusEffect.PLANTED)) GetComponent<Rigidbody2D>().velocity = Vector3.down*5f;
                	status.ApplyStatusEffect(attackData.StatusEffect, (attackData.StatusEffect == PlayerStatusEffect.PLANTED || attackData.StatusEffect == PlayerStatusEffect.AMBERED?
                                                                    attackData.StatusDuration * framerateScalar *2f* 4f/(1f+Mathf.Exp(-0.03f * (drifter.DamageTaken -80f))):
                                                                    attackData.StatusDuration * framerateScalar));


                    //Attatch Defender to attacker's hitbox for grab moves.
                    if(attackData.StatusEffect == PlayerStatusEffect.GRABBED)
                    {
                        status.grabPoint = hitbox.gameObject.GetComponent<Collider2D>();
                        attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,framerateScalar);

                    }              	
                }
                //Pop playewrs out of the ground when they are already grounded
                else if(attackData.StatusEffect == PlayerStatusEffect.PLANTED && !GetComponent<PlayerMovement>().grounded){
                	status.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,6f * framerateScalar);
                }

                //Extend hitpause on kill
                if (willCollideWithBlastZoneAccurate(GetComponent<Rigidbody2D>(), HitstunDuration) && drifter.Stocks <= 1 && NetworkPlayers.Instance.players.Values.Where(x => x != null).ToList().Count <=2)
                {
                    HitstunDuration = 3f;
                    GetComponent<PlayerMovement>().techWindowElapsed = 2f;
                } 
                else if (willCollideWithBlastZone(GetComponent<Rigidbody2D>() , HitstunDuration) ) Mathf.Min(HitstunDuration*=2f,3f);
                
                
                //apply defender hitpause
                if(HitstunDuration>0 && attackData.StatusEffect != PlayerStatusEffect.HITPAUSE )status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,(attackData.HitVisual == HitSpark.CRIT || status.HasStatusEffect(PlayerStatusEffect.ARMOUR)) ? .6f:(damageDealt <=2.5f ? .15f :Mathf.Max(HitstunDuration*.25f ,.25f)));
                StartCoroutine(drifter.GetComponentInChildren<GameObjectShake>().Shake(attackData.StatusEffect != PlayerStatusEffect.CRINGE?HitstunDuration*.2f:attackData.StatusDuration* framerateScalar,attackData.StatusEffect != PlayerStatusEffect.CRINGE?1.5f:2f));

                returnCode = 0;             
            }
            //Normal guarding behavior
            else if(drifter.guarding && !drifter.parrying)
            {
                //push both players back on guarrd

                if(hitbox.gameObject.tag != "Projectile")hitbox.parent.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Clamp(HitstunDuration,.2f,.8f) * hitbox.Facing *-15f, hitbox.parent.GetComponent<Rigidbody2D>().velocity.y);
               
                //No pushback on perfect guard
                if(!drifter.perfectGuarding)
                {
                    // Get new particle for prefect guarda
                    GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Clamp(HitstunDuration,.2f,.8f) *10f  * hitbox.Facing , GetComponent<Rigidbody2D>().velocity.y);
                }

                else GetComponent<PlayerMovement>().spawnJuiceParticle(hitSparkPos, MovementParticleMode.Parry);
                //put defender in blockstun
                if(attackData.HitStun != 0){
                        status?.calculateFrameAdvantage(framerateScalar * (1f + Mathf.Ceil(attackData.AttackDamage/4f)),hitbox.parent.GetComponent<Drifter>().getRemainingAttackTime());
                        status?.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK, framerateScalar * (1f + Mathf.Ceil(attackData.AttackDamage/4f)));
                }

                returnCode = -1; 

            }
            //Parrying a guardbreaker
            else if(drifter.parrying && attackData.isGrab && hitbox.gameObject.tag != "Projectile")
            {

                //STODO Shit out lots of particles here
                if(hitbox.gameObject.tag != "Projectile")hitbox.parent.GetComponent<Rigidbody2D>().velocity = new Vector2(hitbox.Facing *-35f, hitbox.parent.GetComponent<Rigidbody2D>().velocity.y);
               
                GetComponent<Rigidbody2D>().velocity = new Vector2(35f * hitbox.Facing , GetComponent<Rigidbody2D>().velocity.y);
                returnCode = -2;

            }
            //Parrying a normal attack
            else if(drifter.parrying && hitbox.gameObject.tag != "Projectile")
            {
                //TODO Shit out more paricles

                GetComponent<PlayerMovement>().spawnJuiceParticle(hitSparkPos, MovementParticleMode.Parry);

                attackerStatus.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,1f);
                attackerStatus.ApplyStatusEffect(PlayerStatusEffect.CRINGE,1f);
                StartCoroutine(Shake.zoomEffect(.6f,Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),false));
                attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.6f);
                returnCode = -2;

            }
            
            // create hit sparks
            

            //When Guardbroken, play the crit animation
            if(guardbroken) hitSparkMode = HitSpark.CRIT;

            //If a move is guarded successfully, play the relevant block hitspark
            //TODO: update for parries
            else if (drifter != null && drifter.guarding && !attackData.isGrab)hitSparkMode = drifter.BlockReduction > 0.5f ? HitSpark.GUARD_WEAK : HitSpark.GUARD_STRONG;

            //Otherwise, use the attacks hitspark
            else hitSparkMode = attackData.HitVisual;

            //apply attacker hitpause
            if((hitbox.gameObject.tag != "Projectile" || hitSparkMode == HitSpark.CRIT)) attackerStatus.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,(hitSparkMode == HitSpark.CRIT || status.HasStatusEffect(PlayerStatusEffect.ARMOUR))? .6f : (damageDealt <=2.5f ? .14f : Mathf.Max(HitstunDuration*.22f,.19f)));

            

            float hitSparkAngle = facingDir * ((Mathf.Abs(attackData.AngleOfImpact) > 65f && attackData.HitVisual != HitSpark.SPIKE) ? Mathf.Sign(attackData.AngleOfImpact) * 90f : 0f);
            GraphicalEffectManager.Instance.CreateHitSparks(hitSparkMode, hitSparkPos, hitSparkAngle, hitSparkScale);
        
            if (drifter != null && willCollideWithBlastZone(GetComponent<Rigidbody2D>(), HitstunDuration))
            {
                //hitSparkPos = Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f);
                GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.CRIT, hitSparkPos, 0, new Vector2(facingDir * 10f, 10f));

                if(drifter.Stocks <= 1 && willCollideWithBlastZoneAccurate(GetComponent<Rigidbody2D>(), HitstunDuration))
                    StartCoroutine(Shake.zoomEffect(HitstunDuration*.25f,Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),true));
            }

            if(hitSparkMode == HitSpark.CRIT)StartCoroutine(Shake.zoomEffect(.6f,Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),false));

            // Ancillary Hitsparks
            if (drifter != null && damageDealt >0f) StartCoroutine(delayHitsparks(hitSparkPos,attackData.AngleOfImpact,damageDealt,HitstunDuration *.25f));
        }
        return returnCode;
    }

    IEnumerator CleanupOldAttacks()
    {
        while (true)
        {
            yield return new WaitForSeconds(MAX_ATTACK_DURATION);
            // find old attackIDs
            List<int> toRemove = new List<int>();
            foreach (int attackID in oldAttacks.Keys)
            {
                if (Time.time - oldAttacks[attackID] > MAX_ATTACK_DURATION)
                {
                    toRemove.Add(attackID);
                }
            }
            // delete old attackIDs
            foreach (int attackID in toRemove)
            {
                oldAttacks.Remove(attackID);
            }
        }
    }

    protected IEnumerator delayHitsparks(Vector3 position, float angle,float damage, float duration)
    {
        Vector3 hitSparkPos = position;
        float angleT;
        float stepSize = duration / ((damage + 2 )/3);

        if(damage >= 2.5f)GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.RING, position,angle, new Vector2(10f, 10f));

        for (int i = 0; i < (damage + 2 )/3 ; i++)
        {
            angleT = angle + Random.Range(-45, 45);
            hitSparkPos += Quaternion.Euler(0, 0, angleT) * new Vector3(-Random.Range(1, 4), 0, 0);
            GraphicalEffectManager.Instance.CreateHitSparks(randomSpark(), position, angleT, new Vector2(10f, 10f));

            angleT += 180;

            hitSparkPos += Quaternion.Euler(0, 0, angleT) * new Vector3(-Random.Range(1, 4), 0, 0);
            GraphicalEffectManager.Instance.CreateHitSparks(randomSpark(), hitSparkPos, angleT, new Vector2(10f, 10f));

            yield return new WaitForSeconds(stepSize);
        }
        yield break;
    }

    HitSpark randomSpark()
    {
        int index = Random.Range(0, 4);

        //Preferantially spawn oomph sparks
        switch(index)
        {

            case(0):
                return HitSpark.OOMPHSPARK;
            case(1):
                return HitSpark.OOMPHDARK;
            case(2):
                return HitSpark.STAR1;
            case(3):
                return HitSpark.STAR2;
            default:
                return HitSpark.STAR1;
        }

    }

    // Super-armor logic
    protected class AttackEffect
    {
        public Coroutine Effect;
        public float SuperArmor;
        public float Damage;
    }

    List<AttackEffect> movementEffects = new List<AttackEffect>();
    protected void StartMovementEffect(IEnumerator ef, float superArmor)
    {
        if (ef != null)
        {
            movementEffects.Add(new AttackEffect()
            {
                Effect = StartCoroutine(ef),
                SuperArmor = superArmor,
                Damage = 0
            });
        }
    }

    protected bool willCollideWithBlastZone(Rigidbody2D rigidbody, float hitstun)
    {
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



        if (xVel * hitstun >= xDel || yVel * hitstun >= yDel)
            return true;

        return false;
    }

    protected bool willCollideWithBlastZoneAccurate(Rigidbody2D rigidbody, float hitstun)
    {
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

        float g = rigidbody.gravityScale * Physics2D.gravity.y;

        if (Mathf.Sign(rigidbody.velocity.y) == -1)
        {
            g *= -1;
        }

        if (xVel * hitstun >= xDel || yVel * hitstun + (0.5 * g * hitstun * hitstun) >= yDel)
            return true;

        return false;
    }
}
