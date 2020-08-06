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
        // only host processes hits, don't hit ourself, and ignore previously registered attacks
        if (GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && !oldAttacks.ContainsKey(attackID))
        {
            // register new attack
            oldAttacks[attackID] = Time.time;
            // apply hit effects
            hitbox.parent.GetComponent<PlayerAttacks>().Hit(attackType, attackID, hurtbox.parent);
            // apply damage
            Drifter drifter = GetComponent<Drifter>();
            if (drifter != null)
            {
                drifter.DamageTaken += attackData.AttackDamage * (GetComponentInChildren<Animator>().GetBool("Guarding") ? 0.35f : 1f);
            }
            // apply knockback
            float facingDir = Mathf.Sign(hurtbox.parent.transform.position.x - hitbox.parent.transform.position.x);
            facingDir = facingDir == 0 ? 1 : facingDir;
            // rotate direction by angle of impact
            Vector2 forceDir = Quaternion.Euler(0, 0, attackData.AngleOfImpact * facingDir) * (facingDir * Vector2.right);
            // how much damage matters to knockback
            const float ARMOR = 180f;
            float damageMultiplier = drifter != null ? (drifter.DamageTaken + ARMOR) / ARMOR : 1f;
            Animator anim = GetComponent<Animator>();
            if (anim != null && anim.GetBool("Guarding"))
            {
                damageMultiplier = 0f;
            }
            GetComponent<Rigidbody2D>().velocity = forceDir.normalized * attackData.Knockback * 2.35f * damageMultiplier;
            // stun player
            float stunMultiplier = Mathf.Lerp(1f, damageMultiplier, 0.5f);
            GetComponent<PlayerStatus>()?.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK, stunMultiplier * attackData.HitStun);
            DamageSuperArmor(stunMultiplier * attackData.HitStun);
            // create hit sparks
            GameObject hitSparks = Instantiate(Entities.GetEntityPrefab("HitSparks"),
                Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),
                Quaternion.identity);
            hitSparks.GetComponent<HitSparks>().SetAnimation(HitSparksEffect.HIT_SPARKS_1);
            if (attackData.AngleOfImpact > 80f)
            {
                hitSparks.GetComponent<HitSparks>().SetAnimation(HitSparksEffect.HIT_SPARKS_2);
                hitSparks.transform.eulerAngles = new Vector3(0, 0, 90f);
                hitSparks.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }
            else if (attackData.Knockback > 18f)
            {
                hitSparks.GetComponent<HitSparks>().SetAnimation(HitSparksEffect.HIT_SPARKS_2);
                hitSparks.transform.localScale = new Vector3(facingDir * 0.7f, 0.7f, 0.7f);
            }
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
