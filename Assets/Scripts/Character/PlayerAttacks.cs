using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum DrifterAttackType
{
    Null,
    Ground_Q_Side, Ground_Q_Down, Ground_Q_Up, Ground_Q_Neutral,
    Aerial_Q_Side, Aerial_Q_Down, Aerial_Q_Up, Aerial_Q_Neutral,
    W_Side, W_Down, W_Up, W_Neutral,
    E_Side, Aerial_E_Down, E_Up, E_Neutral, Roll
}

[Serializable]
public class SingleAttack
{
    public DrifterAttackType attack;
    public SingleAttackData attackData;
}


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
        { DrifterAttackType.Aerial_Q_Up, "Aerial_Up" },
        { DrifterAttackType.Aerial_Q_Down, "Aerial_Down" },
        { DrifterAttackType.Aerial_Q_Side, "Aerial_Side" },
        { DrifterAttackType.Ground_Q_Up, "Ground_Up" },
        { DrifterAttackType.Ground_Q_Down, "Ground_Down" },
        { DrifterAttackType.Ground_Q_Side, "Ground_Side" },
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
        { DrifterAttackType.Aerial_Q_Up, "Aerial_Up" },
        { DrifterAttackType.Aerial_Q_Down, "Aerial_Down" },
        { DrifterAttackType.Aerial_Q_Side, "Aerial_Side" },
        { DrifterAttackType.Ground_Q_Up, "Ground_Up" },
        { DrifterAttackType.Ground_Q_Down, "Ground_Down" },
        { DrifterAttackType.Ground_Q_Side, "Ground_Side" },
    };

    static int nextID = 0;
    // get a new attack ID
    public static int NextID { get { nextID++; return nextID; } }

    // current attack ID and Type, used for outgoing attacks
    public int AttackID { get; private set; }
    public DrifterAttackType AttackType { get; private set; }
    public List<SingleAttack> AttackMap = new List<SingleAttack>();
    Dictionary<DrifterAttackType,SingleAttackData> Attacks = new Dictionary<DrifterAttackType,SingleAttackData>();
    public int maxRecoveries = 1;
    public int currentRecoveries;
    public bool ledgeHanging = false;
    public int Facing = 0;

    Drifter drifter;
    PlayerStatus status;
    Animator animator;
    IMasterHit hit;

    INetworkSync sync;

    void Awake()
    {
        foreach (SingleAttack attack in AttackMap)
        {
            Attacks[attack.attack] = attack.attackData;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        drifter = GetComponent<Drifter>();
        animator = drifter.animator;
        status = GetComponent<PlayerStatus>();
        hit = GetComponentInChildren<IMasterHit>();
        sync = GetComponent<INetworkSync>();
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
        bool canAct = !status.HasStunEffect() && !animator.GetBool("Guarding") && !ledgeHanging;

        if((animator.GetBool("Grounded") && !status.HasStatusEffect(PlayerStatusEffect.END_LAG)) || status.HasEnemyStunEffect()){
            resetRecovery();
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
                if(drifter.input.MoveY > 0)StartAttack(DrifterAttackType.Ground_Q_Up);
                else if(drifter.input.MoveY < 0)StartAttack(DrifterAttackType.Ground_Q_Down);
                else if(drifter.input.MoveX!=0)StartAttack(DrifterAttackType.Ground_Q_Side);
                else StartAttack(DrifterAttackType.Ground_Q_Neutral);
            }
            else
            {                
                if(drifter.input.MoveY > 0)StartAttack(DrifterAttackType.Aerial_Q_Up);
                else if(drifter.input.MoveY < 0)StartAttack(DrifterAttackType.Aerial_Q_Down);
                else if(drifter.input.MoveX!=0)StartAttack(DrifterAttackType.Aerial_Q_Side);
                else StartAttack(DrifterAttackType.Aerial_Q_Neutral);
            }
        }
    }

    public void resetRecovery(){
        currentRecoveries = maxRecoveries;
    }

    void StartAttack(DrifterAttackType attackType)
    {
        SetHitboxesActive(false);
        drifter.SetAnimatorTrigger(AnimatorTriggers[attackType]);
        SetupAttackID(attackType);

        status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f);
            //Attacks[attackType].EndLag);

        //StartCoroutine(ListenForAttackEvents(attackType));
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
        //FinishAttack(attackType);
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
        yield break;
    }

    public void Hit(DrifterAttackType attackType, int attackID, GameObject target)
    {
        UnityEngine.Debug.Log("HIT DETECTED IN PLAYER ATTACKS");
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
            hitbox.AttackData = Attacks[AttackType];
            hitbox.Active = false;
            hitbox.Facing = Facing;
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
