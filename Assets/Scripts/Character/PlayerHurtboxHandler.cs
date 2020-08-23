using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHurtboxHandler : MonoBehaviour
{
    // keep track of what attacks we've already processed
    // AttackID -> Timestamp
    Dictionary<int, float> oldAttacks = new Dictionary<int, float>();
    const float MAX_ATTACK_DURATION = 10f;

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
        UnityEngine.Debug.Log("ATTACK REGISTERERD");
        // only host processes hits, don't hit ourself, and ignore previously registered attacks
        if (GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && !oldAttacks.ContainsKey(attackID))
        {

            // register new attack
            oldAttacks[attackID] = Time.time;
            // apply hit effects
            hitbox.parent.GetComponent<PlayerAttacks>().Hit(attackType, attackID, hurtbox.parent);

            GetComponent<PlayerStatus>().ApplyStatusEffect(PlayerStatusEffect.HIT,.1f);
            
            

            // apply damage, ignored if invuln
            Drifter drifter = GetComponent<Drifter>();
            if (drifter != null && !GetComponent<PlayerStatus>().HasInulvernability())
            {
                drifter.DamageTaken += attackData.AttackDamage * (drifter.animator.GetBool("Guarding") ? 1 - drifter.BlockReduction : 1f);
            }
            // apply knockback
            float facingDir = Mathf.Sign(hurtbox.parent.transform.position.x - hitbox.parent.transform.position.x);
            facingDir = facingDir == 0 ? 1 : facingDir;
            // rotate direction by angle of impact
            Vector2 forceDir = Quaternion.Euler(0, 0, attackData.AngleOfImpact * facingDir) * (facingDir * Vector2.right);
            // how much damage matters to knockback
            const float ARMOR = 180f;
            float damageMultiplier = drifter != null ? (drifter.DamageTaken + ARMOR) / ARMOR : 1f;
            if (drifter.animator.GetBool("Guarding"))
            {
                damageMultiplier = 0f;
            }
            float stunMultiplier = (drifter.DamageTaken+30)/100f;
            //Ignore knockback if invincible or armoured
            if(!GetComponent<PlayerStatus>().HasInulvernability() && !drifter.animator.GetBool("Guarding")){

                if(attackData.HitStun>=0 && !GetComponent<PlayerStatus>().HasArmour())
                {
                    GetComponent<PlayerStatus>()?.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK, stunMultiplier * attackData.HitStun);
                }
                GetComponent<PlayerStatus>().ApplyStatusEffect(attackData.StatusEffect,attackData.StatusDuration);

                if(!GetComponent<PlayerStatus>().HasArmour())
                {
                    GetComponent<Rigidbody2D>().velocity = forceDir.normalized * (float)((drifter.DamageTaken / 10 + drifter.DamageTaken * attackData.AttackDamage / 20)
                        * 200 / ( ((GetComponent<PlayerStatus>().HasStatusEffect(PlayerStatusEffect.EXPOSED)
                        || GetComponent<PlayerStatus>().HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))?drifter.drifterData.Weight-50:drifter.drifterData.Weight)
                        + 100) * 1.4 * attackData.KnockbackScale + attackData.Knockback);
                }            
            }
            
            DamageSuperArmor(stunMultiplier * attackData.HitStun);
            // create hit sparks
            GameObject hitSparks = Instantiate(Entities.GetEntityPrefab("HitSparks"),
                Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),
                Quaternion.identity);


            UnityEngine.Debug.Log("HITSPARK" + attackData.GetHitSpark());

            if(drifter.animator.GetBool("Guarding")){
                    hitSparks.GetComponent<HitSparks>().SetAnimation(drifter.BlockReduction>.5?6:5);
                    hitSparks.transform.localScale = new Vector3(facingDir * 10f, 10f, 10f);
            }
            else if(attackData.GetHitSpark() != 1){
                hitSparks.GetComponent<HitSparks>().SetAnimation(attackData.GetHitSpark());
                hitSparks.transform.localScale = new Vector3(facingDir *-6f, 6f, 6f);
            }
            else{
                hitSparks.GetComponent<HitSparks>().SetAnimation(attackData.GetHitSpark());
                hitSparks.transform.localScale = new Vector3(facingDir *10f, 10f, 10f);
            }
            
            hitSparks.transform.eulerAngles = new Vector3(0, 0, facingDir * (Mathf.Abs(attackData.AngleOfImpact) > 65f? Mathf.Sign(attackData.AngleOfImpact)*90f:0f));


            Entities.AddEntity(hitSparks);

            
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
}
