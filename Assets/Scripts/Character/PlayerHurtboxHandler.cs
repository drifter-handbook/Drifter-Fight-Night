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
    NetworkHost host;
    ScreenShake Shake;

    // Start is called before the first frame update
    void Start()
    {
        host = GameController.Instance.host;
        Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();

        StartCoroutine(CleanupOldAttacks());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CanHit(int attackID)
    {
        return !oldAttacks.ContainsKey(attackID);
    }

    public void RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, DrifterAttackType attackType, SingleAttackData attackData)
    {
        //UnityEngine.Debug.Log(attackID);
        // only host processes hits, don't hit ourself, and ignore previously registered attacks
        if (GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && !oldAttacks.ContainsKey(attackID))
        {

            // register new attack
            oldAttacks[attackID] = Time.time;
            // apply hit effects
            hitbox.parent.GetComponent<PlayerAttacks>().Hit(attackType, attackID, hurtbox.parent);
            Drifter drifter = GetComponent<Drifter>();
            PlayerStatus status = drifter.status;

            //Ignore the collision if invulnerable
            if(status.HasInulvernability())return;
            
            //Freezefame if hit a counter
            if(hurtbox.gameObject.name == "Counter" &&  attackData.AttackDamage >0f)
            {
                hitbox.parent.GetComponent<PlayerStatus>().ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.4f);
                status.ApplyStatusEffect(PlayerStatusEffect.HIT,.3f);
                return;
            }
           

            // apply damage
            if (drifter != null && status != null)
            {
                drifter.DamageTaken += (attackData.AttackDamage  + (status.HasStatusEffect(PlayerStatusEffect.HEXED) &&  attackData.AttackDamage >0 ? 2f : 0f) )* (drifter.guarding && !attackData.isGrab ? 1 - drifter.BlockReduction : 1f);
                //ScreenShake
            }
            // apply knockback

            float facingDir = Mathf.Sign(hitbox.Facing) == 0 ? 1 : Mathf.Sign(hitbox.Facing) ;

            // rotate direction by angle of impact
            //Do we still need all this math?
            //calculated angle
            float angle = Mathf.Sign(attackData.AngleOfImpact) * Mathf.Atan2(hurtbox.parent.transform.position.y-hitbox.parent.transform.position.y, hurtbox.parent.transform.position.x-hitbox.parent.transform.position.x)*180 / Mathf.PI;
            Vector2 forceDir = Mathf.Abs(attackData.AngleOfImpact) <= 360?
                                    Quaternion.Euler(0, 0, attackData.AngleOfImpact * facingDir) * (facingDir * Vector2.right) :
                                    Quaternion.Euler(0, 0, angle) * Vector2.right;

            float KB = (float)(((drifter.DamageTaken / 10 + drifter.DamageTaken * attackData.AttackDamage / 20)
                        * 200 / (GetComponent<PlayerMovement>().Weight + 100) * 1.4 *
                         ((status.HasStatusEffect(PlayerStatusEffect.EXPOSED) || status.HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))
                            ?1.5f:1)) * attackData.KnockbackScale + attackData.Knockback);

            float HitstunDuration = (attackData.HitStun>=0)?attackData.HitStun:(KB*.006f + .1f);
            float hitstunOriginal = HitstunDuration;


            //Ignore knockback if invincible or armoured
            if (status != null && (attackData.isGrab || !drifter.guarding)){

                if(!status.HasArmour() || attackData.isGrab){

                    if(Shake != null && attackData.Knockback !=0){
                        Shake.CurrentShake = StartCoroutine(Shake.Shake((willCollideWithBlastZone(GetComponent<Rigidbody2D>(), HitstunDuration)?0.3f:0.1f),Mathf.Clamp((((attackData.Knockback - 10)/100f + (attackData.AttackDamage-10)/44f)) * attackData.KnockbackScale,.07f,.8f)));//StartCoroutine(Shake.Shake(drifter.DamageTaken/100f * Mathf.Max((attackData.AttackDamage + attackData.KnockbackScale *3f -3f),.1f)/10f * .1f,Mathf.Max((attackData.AttackDamage+ attackData.KnockbackScale*3f - 3f),.2f)/10f));
                    }            
                    if(attackData.Knockback > 0 && attackData.AngleOfImpact > -361){
                        GetComponent<Rigidbody2D>().velocity = new Vector2(forceDir.normalized.x * KB, GetComponent<PlayerMovement>().grounded?Mathf.Abs(forceDir.normalized.y * KB): forceDir.normalized.y * KB);
                        if(GetComponent<PlayerMovement>().grounded && attackData.AngleOfImpact < -75 &&  attackData.AngleOfImpact > -105)GetComponent<PlayerMovement>().spawnJuiceParticle(transform.position + new Vector3(0,-2.5f,0), MovementParticleMode.Restitution);
                    }
                    else if(attackData.Knockback > 0 && attackData.AngleOfImpact <= -361){
                        GetComponent<Rigidbody2D>().velocity = hitbox.parent.GetComponent<Rigidbody2D>().velocity * (1 + attackData.KnockbackScale);
                    }
                                        
                    if(attackData.HitStun != 0){
                        status?.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK, HitstunDuration);
                    }
                }
                if(attackData.StatusEffect != PlayerStatusEffect.PLANTED || GetComponent<PlayerMovement>().grounded){

                	if(attackData.StatusEffect == PlayerStatusEffect.PLANTED && !status.HasStatusEffect(PlayerStatusEffect.PLANTED)) GetComponent<Rigidbody2D>().velocity = Vector3.down*5f;
                	status.ApplyStatusEffect(attackData.StatusEffect, (attackData.StatusEffect == PlayerStatusEffect.PLANTED || attackData.StatusEffect == PlayerStatusEffect.AMBERED?
                                                                    attackData.StatusDuration *2f* 4f/(1f+Mathf.Exp(-0.03f * (drifter.DamageTaken -80f))):
                                                                    attackData.StatusDuration));

                	
                }
                else if(attackData.StatusEffect == PlayerStatusEffect.PLANTED && !GetComponent<PlayerMovement>().grounded){
                	status.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,.4f);
                }

                //apply defender hitpause
                hitstunOriginal = HitstunDuration;
                if (willCollideWithBlastZoneAccurate(GetComponent<Rigidbody2D>(), hitstunOriginal) && drifter.Stocks <= 1 && NetworkPlayers.Instance.players.Values.Where(x => x != null).ToList().Count <=2) HitstunDuration = 3f;
                else if (willCollideWithBlastZone(GetComponent<Rigidbody2D>() , hitstunOriginal) ) Mathf.Min(HitstunDuration*=2f,3f);
                
                

                if(HitstunDuration>0 && attackData.StatusEffect != PlayerStatusEffect.HITPAUSE)status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,attackData.HitVisual == HitSpark.CRIT?.6f:Mathf.Max(HitstunDuration*.25f ,.1f));
                StartCoroutine(drifter.GetComponentInChildren<GameObjectShake>().Shake(attackData.StatusEffect != PlayerStatusEffect.HITPAUSE?HitstunDuration*.2f:attackData.StatusDuration,attackData.StatusEffect != PlayerStatusEffect.HITPAUSE?1.5f:2.5f));

                //Cape logic
                if(attackData.StatusEffect == PlayerStatusEffect.REVERSED)
                {
                    Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
                    velocity = new Vector2(-1 * velocity.x,velocity.y);
                    GetComponent<PlayerMovement>().flipFacing();
                }            
            }
            else if(drifter.guarding)
            {
                //push bloth players back on guarrd

                if(hitbox.gameObject.tag != "Projectile")hitbox.parent.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Clamp(HitstunDuration,.2f,.8f) * hitbox.Facing *-45f, hitbox.parent.GetComponent<Rigidbody2D>().velocity.y);
               
                GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Clamp(HitstunDuration,.2f,.8f) *40f  * hitbox.Facing , GetComponent<Rigidbody2D>().velocity.y);

            }

            //apply attacker hitpause
            if(hitbox.gameObject.tag != "Projectile" || attackData.HitVisual == HitSpark.CRIT)hitbox.parent.GetComponent<PlayerStatus>().ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,attackData.HitVisual == HitSpark.CRIT ? .6f : Mathf.Max(HitstunDuration*.22f,.1f));

            // create hit sparks
            Vector3 hitSparkPos = Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f);
            HitSpark hitSparkMode = HitSpark.POKE;
            Vector2 hitSparkScale = Vector2.one;
            if (drifter != null && drifter.guarding && !attackData.isGrab){
                hitSparkMode = drifter.BlockReduction > 0.5f ? HitSpark.GUARD_WEAK : HitSpark.GUARD_STRONG;
                hitSparkScale = new Vector2(facingDir * 10f, 10f);
            }
            else if(drifter != null && attackData.HitVisual != HitSpark.POKE && attackData.HitVisual != HitSpark.MAGICWEAK)
            {
                hitSparkMode = attackData.HitVisual;
                hitSparkScale = new Vector2(facingDir *-6f, 6f);
            }
            else{
                hitSparkMode = attackData.HitVisual;
                hitSparkScale = new Vector2(facingDir *10f, 10f);
            }
            float hitSparkAngle = facingDir * ((Mathf.Abs(attackData.AngleOfImpact) > 65f && attackData.HitVisual != HitSpark.SPIKE) ? Mathf.Sign(attackData.AngleOfImpact) * 90f : 0f);
            GraphicalEffectManager.Instance.CreateHitSparks(hitSparkMode, hitSparkPos, hitSparkAngle, hitSparkScale);
        
            if (drifter != null && willCollideWithBlastZone(GetComponent<Rigidbody2D>(), hitstunOriginal))
            {
                hitSparkPos = Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f);
                GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.CRIT, hitSparkPos, 0, new Vector2(facingDir * 10f, 10f));

                if(drifter.Stocks <= 1 && willCollideWithBlastZoneAccurate(GetComponent<Rigidbody2D>(), hitstunOriginal))
                    StartCoroutine(Shake.zoomEffect(HitstunDuration*.25f,Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),true));
            }

            if(attackData.HitVisual == HitSpark.CRIT && ! drifter.guarding)
            {
                StartCoroutine(Shake.zoomEffect(.6f,Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),false));
            }


            // Ancillary Hitsparks
            if (drifter != null && attackData.AttackDamage >0f) StartCoroutine(delayHitsparks(Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),attackData.AngleOfImpact,attackData.AttackDamage,HitstunDuration *.25f));
        }
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

    IEnumerator delayHitsparks(Vector3 position, float angle,float damage, float duration)
    {
        Vector3 hitSparkPos = position;
        float angleT;
        float stepSize = duration / ((damage + 3) / 5);


        for (int i = 0; i < (damage + 2) / 4; i++)
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
        int index = Random.Range(0, 11);

        //Preferantially spawn oomph sparks
        if(index <= 4 ) return HitSpark.OOMPHSPARK;
        else if(index >= 6 ) return HitSpark.OOMPHDARK;

        //1/11 are stars
        return HitSpark.STAR_FAST;
    }

    // Super-armor logic
    private class AttackEffect
    {
        public Coroutine Effect;
        public float SuperArmor;
        public float Damage;
    }

    List<AttackEffect> movementEffects = new List<AttackEffect>();
    private void StartMovementEffect(IEnumerator ef, float superArmor)
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

    // call this when launched to damage a movement effect
    public void DamageSuperArmor(float damage)
    {
        for (int i = 0; i < movementEffects.Count; i++)
        {
            AttackEffect ef = movementEffects[i];
            ef.Damage += damage;
            if (ef.Damage > ef.SuperArmor)
            {
                if (ef.Effect != null)
                {
                    StopCoroutine(ef.Effect);
                }
                // TODO: do something
                movementEffects.RemoveAt(i);
                i--;
            }
        }
    }

    private bool willCollideWithBlastZone(Rigidbody2D rigidbody, float hitstun)
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

    private bool willCollideWithBlastZoneAccurate(Rigidbody2D rigidbody, float hitstun)
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
