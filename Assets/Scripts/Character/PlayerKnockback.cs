﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    static int nextID = 0;
    // get a new attack ID
    public static int NextID { get { nextID++; return nextID; } }

    // current attack ID and Type, used for outgoing attacks
    public int AttackID { get; private set; }
    public PlayerAttackType AttackType { get; private set; }

    // keep track of what attacks we've already processed
    // AttackID -> Timestamp
    Dictionary<int, float> oldAttacks = new Dictionary<int, float>();
    const float MAX_ATTACK_DURATION = 10f;

    // for creating hitsparks
    NetworkEntityList Entities;

    // ignore all hits if client
    bool IsHost = false;

    // Start is called before the first frame update
    void Start()
    {
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        StartCoroutine(CleanupOldAttacks());
        IsHost = GameController.Instance.GetComponent<GameSyncManager>().IsHost;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PerformAttack(PlayerAttackType attackType)
    {
        AttackType = attackType;
        AttackID = NextID;
        foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>())
        {
            hitbox.AttackID = AttackID;
            hitbox.AttackType = AttackType;
        }
    }
    // called by hitboxes during attack animation
    // reset attack ID, allowing for another hit in the same attack animation
    public void MultiHitAttack()
    {
        AttackID = NextID;
        foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>())
        {
            hitbox.AttackID = AttackID;
        }
    }

    public void RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, SingleAttackData attackData)
    {
        // only host processes hits, don't hit ourself, and ignore previously registered attacks
        if (IsHost && hitbox.parent != hurtbox.parent && !oldAttacks.ContainsKey(attackID))
        {
            // register new attack
            oldAttacks[attackID] = Time.time;
            // apply damage
            Drifter drifter = GetComponent<Drifter>();
            if (drifter != null)
            {
                drifter.DamageTaken += attackData.AttackDamage * (GetComponent<PlayerMovement>().input.Guard ? 0.35f : 1f);
            }
            // apply knockback
            int facingDir = hitbox.parent.GetComponent<PlayerMovement>().Facing;
            // rotate direction by angle of impact
            Vector2 forceDir = Quaternion.Euler(0, 0, attackData.AngleOfImpact * facingDir) * (facingDir * Vector2.right);
            // how much damage matters to knockback
            const float ARMOR = 180f;
            float damageMultiplier = drifter != null ? (drifter.DamageTaken + ARMOR) / ARMOR : 1f;
            if (GetComponent<PlayerMovement>().input.Guard)
            {
                damageMultiplier = 0f;
            }
            GetComponent<Rigidbody2D>().velocity = forceDir.normalized * attackData.Knockback * 4f * damageMultiplier;
            // create hit sparks
            GameObject hitSparks = Instantiate(Entities.GetEntityPrefab("HitSparks"),
                Vector3.Lerp(hurtbox.parent.transform.position, hitbox.parent.transform.position, 0.1f),
                Quaternion.identity);
            hitSparks.GetComponent<HitSparks>().SetAnimation(HitSparksEffect.HIT_SPARKS_1);
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
}
