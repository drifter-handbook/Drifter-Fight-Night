﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour
{
    public static Dictionary<DrifterAttackType, string> AnimatorTriggers = new Dictionary<DrifterAttackType, string>()
    {
        { DrifterAttackType.Ground_Q_Neutral, "Attack" },
        { DrifterAttackType.Aerial_Q_Neutral, "Aerial" },
        { DrifterAttackType.W_Up, "Recovery" },
        { DrifterAttackType.E_Side, "Grab" },
        { DrifterAttackType.W_Neutral, "W_Neutral" },
        { DrifterAttackType.W_Side, "W_Side" },
        { DrifterAttackType.W_Down, "W_Down" },
        { DrifterAttackType.Roll, "Roll" },
    };
    public static Dictionary<DrifterAttackType, string> AnimatorStates = new Dictionary<DrifterAttackType, string>()
    {
        { DrifterAttackType.Ground_Q_Neutral, "Attack" },
        { DrifterAttackType.Aerial_Q_Neutral, "Aerial" },
        { DrifterAttackType.W_Up, "Recovery" },
        { DrifterAttackType.E_Side, "Grab" },
        { DrifterAttackType.W_Neutral, "W_Neutral" },
        { DrifterAttackType.W_Side, "W_Side" },
        { DrifterAttackType.W_Down, "W_Down" },
        { DrifterAttackType.Roll, "Roll" },
    };

    static int nextID = 0;
    // get a new attack ID
    public static int NextID { get { nextID++; return nextID; } }

    // current attack ID and Type, used for outgoing attacks
    public int AttackID { get; private set; }
    public DrifterAttackType AttackType { get; private set; }
    public int maxRecoveries = 1;
    int currentRecoveries;

    Drifter drifter;
    PlayerStatus status;
    Animator animator;
    IMasterHit hit;
    DrifterAttackData attackData;

    INetworkSync sync;

    // Start is called before the first frame update
    void Start()
    {
        drifter = GetComponent<Drifter>();
        animator = drifter.animator;
        status = GetComponent<PlayerStatus>();
        hit = GetComponentInChildren<IMasterHit>();
        sync = GetComponent<INetworkSync>();
        attackData = GameController.Instance.AllData.GetAttacks(sync.Type);
        currentRecoveries = maxRecoveries;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameController.Instance.IsHost || GameController.Instance.IsPaused)
        {
            return;
        }

        // get input
        bool lightPressed = !drifter.prevInput.Light && drifter.input.Light;
        bool specialPressed = !drifter.prevInput.Special && drifter.input.Special;
        bool grabPressed = !drifter.prevInput.Grab && drifter.input.Grab;
        bool canAct = !status.HasStunEffect() && !animator.GetBool("Guarding");

        if(animator.GetBool("Grounded") && !status.HasStatusEffect(PlayerStatusEffect.END_LAG) || status.HasEnemyStunEffect()){
            currentRecoveries = maxRecoveries;
        }

        if (grabPressed && canAct)
        {
            StartAttack(DrifterAttackType.E_Side);
        }
        else if (specialPressed && drifter.input.MoveY > 0 && canAct && currentRecoveries >0)
        {
            // recovery
            StartAttack(DrifterAttackType.W_Up);
            currentRecoveries--;
        }
        else if (specialPressed && drifter.input.MoveY < 0 && canAct)
        {
            // Down W
            StartAttack(DrifterAttackType.W_Down);
        }
        else if (specialPressed && drifter.input.MoveX != 0 && canAct)
        {
            // Side W
            StartAttack(DrifterAttackType.W_Side);
        }
        else if (specialPressed && drifter.input.MoveX == 0 && drifter.input.MoveY == 0 && canAct)
        {
            // Neutral W
            StartAttack(DrifterAttackType.W_Neutral);
        }
        //attack  //neutral aerial
        else if (lightPressed && canAct)
        {
            if (animator.GetBool("Grounded"))
            {
                StartAttack(DrifterAttackType.Ground_Q_Neutral);
            }
            else
            {
                StartAttack(DrifterAttackType.Aerial_Q_Neutral);
            }
        }
    }

    void StartAttack(DrifterAttackType attackType)
    {
        switch (attackType)
        {
            case DrifterAttackType.Ground_Q_Neutral:
                hit?.callTheLight();
                break;
            case DrifterAttackType.Aerial_Q_Neutral:
                hit?.callTheAerial();
                break;
            case DrifterAttackType.W_Up:
                hit?.callTheRecovery();
                break;
            case DrifterAttackType.E_Side:
                hit?.callTheGrab();
                break;
            case DrifterAttackType.W_Neutral:
                hit?.callTheNeutralW();
                break;
            case DrifterAttackType.W_Side:
                hit?.callTheSideW();
                break;
            case DrifterAttackType.W_Down:
                hit?.callTheDownW();
                break;
            case DrifterAttackType.Roll:
                hit?.callTheRoll();
                break;    
        }
        SetHitboxesActive(false);
        drifter.SetAnimatorTrigger(AnimatorTriggers[attackType]);
        SetupAttackID(attackType);
        status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,
            attackData[attackType].EndLag);
        StartCoroutine(ListenForAttackEvents(attackType));
    }
    public IEnumerator ListenForAttackEvents(DrifterAttackType attackType)
    {
        // check for when animator state changes.
        // If animator states aren't named properly, hits won't be detected,
        // FinishAttack will never be called,
        // and we will have a memory and performance leak.
        // Make sure they're named as described in AnimatorStates!

        // enter the state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(AnimatorStates[attackType]))
        {
            yield return null;
        }
        SetHitboxesActive(true);
        // exit the state
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(AnimatorStates[attackType]))
        {
            yield return null;
        }
        FinishAttack(attackType);
        yield break;
    }
    public void Hit(DrifterAttackType attackType, int attackID, GameObject target)
    {
        switch (attackType)
        {
            case DrifterAttackType.Ground_Q_Neutral:
                hit?.hitTheLight(target);
                break;
            case DrifterAttackType.Aerial_Q_Neutral:
                hit?.hitTheAerial(target);
                break;
            case DrifterAttackType.W_Up:
                hit?.hitTheRecovery(target);
                break;
            case DrifterAttackType.E_Side:
                hit?.hitTheGrab(target);
                break;
            case DrifterAttackType.W_Neutral:
                hit?.hitTheNeutralW(target);
                break;
            case DrifterAttackType.W_Side:
                hit?.hitTheSideW(target);
                break;
            case DrifterAttackType.W_Down:
                hit?.hitTheDownW(target);
                break;
            case DrifterAttackType.Roll:
                hit?.hitTheRoll(target);
                break;       
        }
    }
    void FinishAttack(DrifterAttackType attackType)
    {
        switch (attackType)
        {
            case DrifterAttackType.Ground_Q_Neutral:
                hit?.cancelTheLight();
                break;
            case DrifterAttackType.Aerial_Q_Neutral:
                hit?.cancelTheAerial();
                break;
            case DrifterAttackType.W_Up:
                hit?.cancelTheRecovery();
                break;
            case DrifterAttackType.E_Side:
                hit?.cancelTheGrab();
                break;
            case DrifterAttackType.W_Neutral:
                hit?.cancelTheNeutralW();
                break;
            case DrifterAttackType.W_Side:
                hit?.cancelTheSideW();
                break;
            case DrifterAttackType.W_Down:
                hit?.cancelTheDownW();
                break;
            case DrifterAttackType.Roll:
                hit?.cancelTheRoll();
                break;   
        }
        SetHitboxesActive(false);
    }

    public void SetupAttackID(DrifterAttackType attackType)
    {
        AttackType = attackType;
        AttackID = NextID;
        foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.GetComponent<Collider2D>().enabled = false;
            hitbox.AttackID = AttackID;
            hitbox.AttackType = AttackType;
            hitbox.Active = false;
        }
    }
    // called by hitboxes during attack animation
    // reset attack ID, allowing for another hit in the same attack animation
    public void SetMultiHitAttackID()
    {
        AttackID = NextID;
        foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.AttackID = AttackID;
        }
    }
    // set hitboxes
    public void SetHitboxesActive(bool active)
    {
        foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.Active = active;
        }
    }
}
