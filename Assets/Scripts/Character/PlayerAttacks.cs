using System.Collections;
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
    };
    public static Dictionary<DrifterAttackType, string> AnimatorStates = new Dictionary<DrifterAttackType, string>()
    {
        { DrifterAttackType.Ground_Q_Neutral, "Attack" },
        { DrifterAttackType.Aerial_Q_Neutral, "Aerial" },
        { DrifterAttackType.W_Up, "Recovery" },
        { DrifterAttackType.E_Side, "Grab" },
    };

    static int nextID = 0;
    // get a new attack ID
    public static int NextID { get { nextID++; return nextID; } }

    // current attack ID and Type, used for outgoing attacks
    public int AttackID { get; private set; }
    public DrifterAttackType AttackType { get; private set; }

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

        if (grabPressed && canAct)
        {
            StartAttack(DrifterAttackType.E_Side);
        }
        if (specialPressed && drifter.input.MoveY > 0 && canAct)
        {
            // recovery
            StartAttack(DrifterAttackType.W_Up);
        }
        //attack  //neutral aerial
        if (lightPressed && canAct)
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
