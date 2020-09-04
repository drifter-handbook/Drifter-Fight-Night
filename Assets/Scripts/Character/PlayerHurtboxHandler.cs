using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHurtboxHandler : MonoBehaviour
{
    // keep track of what attacks we've already processed
    // AttackID -> Timestamp
    public Dictionary<int, float> oldAttacks = new Dictionary<int, float>();
    const float MAX_ATTACK_DURATION = 7f;

    // for creating hitsparks
    NetworkEntityList Entities;

    // Start is called before the first frame update
    void Start()
    {
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();

        StartCoroutine(CleanupOldAttacks());
    }

    // Update is called once per frame
    void Update()
    {

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

            PlayerStatus status = GetComponent<PlayerStatus>();
            status.ApplyStatusEffect(PlayerStatusEffect.HIT,.3f);

            // apply damage, ignored if invuln
            Drifter drifter = GetComponent<Drifter>();
            if (drifter != null && status != null && !status.HasInulvernability())
            {
                drifter.DamageTaken += attackData.AttackDamage * (drifter.animator.GetBool("Guarding") && !attackData.isGrab ? 1 - drifter.BlockReduction : 1f);
            }
            // apply knockback
            float facingDir = Mathf.Sign(hurtbox.parent.transform.position.x - hitbox.parent.transform.position.x);
            facingDir = facingDir == 0 ? 1 : facingDir;
            // rotate direction by angle of impact
            //calculated angle
            float angle = Mathf.Sign(attackData.AngleOfImpact) * Mathf.Atan2(hurtbox.parent.transform.position.y-hitbox.parent.transform.position.y, hurtbox.parent.transform.position.x-hitbox.parent.transform.position.x)*180 / Mathf.PI;
            Vector2 forceDir = Mathf.Abs(attackData.AngleOfImpact) <= 360?
                                    Quaternion.Euler(0, 0, attackData.AngleOfImpact * facingDir) * (facingDir * Vector2.right) :
                                    Quaternion.Euler(0, 0, angle) * Vector2.right;


            //Ignore knockback if invincible or armoured
            float KB = 0;
            if(status != null && !status.HasInulvernability() && (attackData.isGrab || !drifter.animator.GetBool("Guarding"))){

                KB = (float)(((drifter.DamageTaken / 10 + drifter.DamageTaken * attackData.AttackDamage / 20)
                        * 200 / (GetComponent<PlayerMovement>().Weight + 100) * 1.4 *
                         ((status.HasStatusEffect(PlayerStatusEffect.EXPOSED) || status.HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))
                            ?1.5f:1)) * attackData.KnockbackScale + attackData.Knockback);

                if(!status.HasArmour() || attackData.isGrab){
                    
                    if(attackData.KnockbackScale >= -1){
                        GetComponent<Rigidbody2D>().velocity = new Vector2(forceDir.normalized.x * KB, GetComponent<PlayerMovement>().grounded?Mathf.Abs(forceDir.normalized.y * KB): forceDir.normalized.y * KB);
                    }
                                        
                    if(attackData.HitStun != 0){
                        status?.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK, (attackData.HitStun>0)?attackData.HitStun:(KB*.0055f + .1f));
                    }
                }

                status.ApplyStatusEffect(attackData.StatusEffect, (attackData.StatusEffect == PlayerStatusEffect.PLANTED || attackData.StatusEffect == PlayerStatusEffect.AMBERED?
                                                                    attackData.StatusDuration *2f* 4f/(1f+Mathf.Exp(-0.03f * (drifter.DamageTaken -80f))):
                                                                    attackData.StatusDuration));            
            }
            // create hit sparks
            GameObject hitSparks = Instantiate(Entities.GetEntityPrefab("HitSparks"),
                Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),
                Quaternion.identity);

            if(drifter != null && drifter.animator.GetBool("Guarding") && !attackData.isGrab){
                    hitSparks.GetComponent<HitSparks>().SetAnimation(drifter.BlockReduction>.5?6:5);
                    hitSparks.transform.localScale = new Vector3(facingDir * 10f, 10f, 10f);
            } else if(drifter != null && attackData.GetHitSpark() != 1 && attackData.GetHitSpark() != 8){
                hitSparks.GetComponent<HitSparks>().SetAnimation(attackData.GetHitSpark());
                hitSparks.transform.localScale = new Vector3(facingDir *-6f, 6f, 6f);
            }
            else{
                hitSparks.GetComponent<HitSparks>().SetAnimation(attackData.GetHitSpark());
                hitSparks.transform.localScale = new Vector3(facingDir *10f, 10f, 10f);
            }

            hitSparks.transform.eulerAngles = new Vector3(0, 0, facingDir * ((Mathf.Abs(attackData.AngleOfImpact) > 65f && attackData.GetHitSpark() != 7)? Mathf.Sign(attackData.AngleOfImpact)*90f:0f));


            Entities.AddEntity(hitSparks);
        
            if (drifter != null && willCollideWithBlastZone(GetComponent<Rigidbody2D>(), (attackData.HitStun > 0) ? attackData.HitStun : (KB * .0055f + .1f)))
            {
                GameObject hitSparkKill = Instantiate(Entities.GetEntityPrefab("HitSparks"),
                Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),
                Quaternion.identity);
                hitSparkKill.GetComponent<HitSparks>().SetAnimation(9);
                hitSparkKill.transform.localScale = new Vector3(facingDir * 10f, 10f, 10f);
                Entities.AddEntity(hitSparkKill);
            }

            if (drifter != null)
            {
                for (int i = 0; i < (attackData.AttackDamage + 2) / 5; i++)
                {
                    float angleT = attackData.AngleOfImpact + UnityEngine.Random.Range(-45, 45);

                    GameObject hitSparkTri1 = Instantiate(Entities.GetEntityPrefab("HitSparks"),
                    Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),
                    Quaternion.identity);
                    hitSparkTri1.GetComponent<HitSparks>().SetAnimation(11);
                    hitSparkTri1.transform.localScale = new Vector3(10f, 10f, 10f);
                    hitSparkTri1.transform.position += Quaternion.Euler(0, 0, angleT) * new Vector3(-UnityEngine.Random.Range(2, 5), 0, 0);
                    hitSparkTri1.transform.rotation = Quaternion.Euler(0, 0, angleT);
                    Entities.AddEntity(hitSparkTri1);

                    angleT += 180;

                    GameObject hitSparkTri2 = Instantiate(Entities.GetEntityPrefab("HitSparks"),
                    Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),
                    Quaternion.identity);
                    hitSparkTri2.GetComponent<HitSparks>().SetAnimation(11);
                    hitSparkTri2.transform.localScale = new Vector3(10f, 10f, 10f);
                    hitSparkTri2.transform.position += Quaternion.Euler(0, 0, angleT) * new Vector3(-UnityEngine.Random.Range(2, 5), 0, 0);
                    hitSparkTri2.transform.rotation = Quaternion.Euler(0, 0, angleT);
                    Entities.AddEntity(hitSparkTri2);
                }
            }
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
}
