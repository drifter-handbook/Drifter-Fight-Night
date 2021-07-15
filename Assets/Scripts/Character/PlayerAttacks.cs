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
    E_Side, E_Air, Roll, Super_Cancel
}

[Serializable]
public class SingleAttack
{
    public DrifterAttackType attack;
    public SingleAttackData attackData;
}

public class PlayerAttacks : MonoBehaviour
{
    public static Dictionary<DrifterAttackType, string> AnimatorStates = new Dictionary<DrifterAttackType, string>()
    {
        { DrifterAttackType.Ground_Q_Neutral, "Ground_Neutral" },
        { DrifterAttackType.Aerial_Q_Neutral, "Aerial_Neutral" },
        { DrifterAttackType.W_Up, "W_Up" },
        { DrifterAttackType.E_Side, "Grab_Ground"},
        { DrifterAttackType.E_Air, "Grab_Air" },
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
        { DrifterAttackType.Super_Cancel, "Super_Cancel" },
    };

    static int nextID = 0;
    // get a new attack ID
    public static int NextID { get { nextID++; return nextID; } }

    // current attack ID and Type, used for outgoing attacks
    public int AttackID { get; private set; }
    public DrifterAttackType AttackType { get; private set; }
    public List<SingleAttack> AttackMap = new List<SingleAttack>();
    public Dictionary<DrifterAttackType,SingleAttackData> Attacks = new Dictionary<DrifterAttackType,SingleAttackData>();

    //[Help("Declares if any specials other than Up-W consume and require a recovery charge", UnityEditor.MessageType.Info)]

    public bool W_Neutral_Is_Recovery = false;
    public bool W_Down_Is_Recovery = false;
    public bool W_Side_Is_Recovery = false;

    public int maxRecoveries = 1;


    [NonSerialized]
    public int currentRecoveries;
    [NonSerialized]
    public bool ledgeHanging = false;
    [NonSerialized]
    public int Facing = 0;

    Drifter drifter;
    PlayerStatus status;
    Animator animator;
    IMasterHit hit;
    PlayerMovement movement;

    NetworkSync sync;

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
        movement = GetComponent<PlayerMovement>();
        hit = GetComponentInChildren<IMasterHit>();
        sync = GetComponent<NetworkSync>();
        currentRecoveries = maxRecoveries;
    }

    // Update is called once per frame
    public void UpdateInput()
    {
        if (!GameController.Instance.IsHost || GameController.Instance.IsPaused)
        {
            return;
        }

        // get input

        bool lightPressed = !drifter.prevInput[1].Light && drifter.prevInput[0].Light && drifter.input.Light;
        bool specialPressed = !drifter.prevInput[1].Special &&  drifter.prevInput[0].Special && drifter.input.Special;

        bool superPressed = !drifter.prevInput[0].Super  && drifter.input.Super;

        bool grabPressed = 

        !drifter.prevInput[0].Light && drifter.input.Light && drifter.prevInput[0].Special ||
        !drifter.prevInput[0].Special && drifter.input.Special && drifter.prevInput[0].Light ||
        !drifter.prevInput[0].Light && drifter.input.Light && !drifter.prevInput[0].Special && drifter.input.Special;



        bool canAct = !status.HasStunEffect() && !drifter.guarding && !ledgeHanging;
        bool canSpecial = !status.HasStunEffect() && !ledgeHanging;

        if((movement.grounded && !status.HasStatusEffect(PlayerStatusEffect.END_LAG)) || status.HasEnemyStunEffect()){
            resetRecovery();
        }

        if(superPressed)
        {
            SetupAttackID(DrifterAttackType.Super_Cancel);
            movement.superCancel();
        }

        else if (grabPressed && canAct)
        {
            if (movement.grounded)StartAttack(DrifterAttackType.E_Side);
            else
            {
                movement.canLandingCancel = true;
                StartAttack(DrifterAttackType.E_Air);  
            } 
        }
        else if(specialPressed && canSpecial)
        {
            if(drifter.input.MoveY > 0 && currentRecoveries >0)
            {
                StartAttack(DrifterAttackType.W_Up);
                currentRecoveries--;
            }
            else if((!W_Down_Is_Recovery || currentRecoveries >0) && drifter.input.MoveY < 0)
            {
                StartAttack(DrifterAttackType.W_Down);
                if(W_Down_Is_Recovery)currentRecoveries--;
            }
            else if((!W_Side_Is_Recovery || currentRecoveries >0) &&drifter.input.MoveX!=0)
            {
                StartAttack(DrifterAttackType.W_Side);
                if(W_Side_Is_Recovery)currentRecoveries--;
            }
            else if((!W_Neutral_Is_Recovery || currentRecoveries >0) &&drifter.input.MoveY==0 && drifter.input.MoveX==0)
            {
                StartAttack(DrifterAttackType.W_Neutral);
                if(W_Neutral_Is_Recovery)currentRecoveries--;
                
            }
        }

        //attack  //neutral aerial
        else if (lightPressed && canAct)
        {
            if (movement.grounded)
            {
                if(drifter.input.MoveY > 0)StartAttack(DrifterAttackType.Ground_Q_Up);
                else if(drifter.input.MoveY < 0)StartAttack(DrifterAttackType.Ground_Q_Down);
                else if(drifter.input.MoveX!=0)StartAttack(DrifterAttackType.Ground_Q_Side);
                else StartAttack(DrifterAttackType.Ground_Q_Neutral);
            }
            else
            {    
                movement.canLandingCancel = true;            
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
        drifter.gainSuperMeter(.05f);
        //UnityEngine.Debug.Log("STARTING ATTACK: " + attackType.ToString());
        SetHitboxesActive(false);
        status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,8f);
        drifter.PlayAnimation(AnimatorStates[attackType]);
        SetupAttackID(attackType);

        
        
    }

    public void Hit(DrifterAttackType attackType, int attackID, GameObject target)
    {
        //UnityEngine.Debug.Log("HIT DETECTED IN PLAYER ATTACKS");
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
