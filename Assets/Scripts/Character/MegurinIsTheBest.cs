using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
//public enum DrifterAttackType
//{
  //  Null,
    //Ground_Q_Side, Ground_Q_Down, Ground_Q_Up, Ground_Q_Neutral,
    //Aerial_Q_Side, Aerial_Q_Down, Aerial_Q_Up, Aerial_Q_Neutral,
    //W_Side, W_Down, W_Up, W_Neutral,
    //E_Side, Aerial_E_Down, E_Up, E_Neutral, Roll
//}

[Serializable]
public class SingleAttack1
{
    public DrifterAttackType attack;
    public SingleAttackData attackData;
}


public class MegurinIsTheBest : MonoBehaviour
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
    public List<SingleAttack1> AttackMap = new List<SingleAttack1>();
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
        foreach (SingleAttack1 attack in AttackMap)
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
        //bool rollPressed = !drifter.prevInput.Roll && drifter.input.Roll;
        bool canAct = !status.HasStunEffect() && !animator.GetBool("Guarding") && !ledgeHanging;


        //Testing shit here:
        //bool dashPressed = !drifter.prevInput.Dash && drifter.input.Dash;

        //if (dashPressed && canAct){
            //drifter.animator.Play("NeroTesting");
        //}
        if((animator.GetBool("Grounded") && !status.HasStatusEffect(PlayerStatusEffect.END_LAG)) || status.HasEnemyStunEffect()){
            resetRecovery();
        }
        if (grabPressed && canAct){
            SetHitboxesActive(false);
            drifter.animator.Play("Grab");
            SetupAttackID(DrifterAttackType.E_Side);
            status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f);
        }
        else if (specialPressed && drifter.input.MoveY> 0 && canAct && currentRecoveries > 0){
            Debug.Log("test");
            SetHitboxesActive(false);
            drifter.animator.Play("Up_W");
            SetupAttackID(DrifterAttackType.W_Up);
            status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f);
            currentRecoveries--;
        }
        else if (specialPressed && drifter.input.MoveY < 0 && canAct){
            // Down W - need to setup the correct play state for the multilayered move
            SetHitboxesActive(false);
            drifter.animator.Play("WDown1");
            SetupAttackID(DrifterAttackType.W_Down);
            status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f);
        }
        else if (specialPressed && drifter.input.MoveX != 0 && canAct){
            // Side W
            SetHitboxesActive(false);
            drifter.animator.Play("W_Side");
            SetupAttackID(DrifterAttackType.W_Side);
            status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f);
        }
        else if (specialPressed && drifter.input.MoveX == 0 && drifter.input.MoveY == 0 && canAct){
             //Neutral W - need to setup the correct play state for the multilayered move
            SetHitboxesActive(false);
            drifter.animator.Play("NeutralWCharge");
            SetupAttackID(DrifterAttackType.W_Neutral);
            status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
        }
        else if (lightPressed && canAct)
        {
            if (animator.GetBool("Grounded"))
            {
                if(drifter.input.MoveY > 0){
                    SetHitboxesActive(false);
                    drifter.animator.Play("Ground_Up");
                    SetupAttackID(DrifterAttackType.Ground_Q_Up);
                    status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
                }
                else if(drifter.input.MoveY < 0){
                    SetHitboxesActive(false);
                    drifter.animator.Play("Ground_Down");
                    SetupAttackID(DrifterAttackType.Ground_Q_Down);
                    status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
                }
                else if(drifter.input.MoveX!=0){
                    SetHitboxesActive(false);
                    drifter.animator.Play("Ground_Side");
                    SetupAttackID(DrifterAttackType.Ground_Q_Side);
                    status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
                }
                else {
                    SetHitboxesActive(false);
                    drifter.animator.Play("Attack");
                    SetupAttackID(DrifterAttackType.Ground_Q_Neutral);
                    status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
                }
            }
            else
            {
                if(drifter.input.MoveY > 0){
                    SetHitboxesActive(false);
                    drifter.animator.Play("W_Up");
                    SetupAttackID(DrifterAttackType.Aerial_Q_Up);
                    status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
                }
                else if(drifter.input.MoveY < 0){
                    SetHitboxesActive(false);
                    drifter.animator.Play("W_Down_Start");
                    SetupAttackID(DrifterAttackType.Aerial_Q_Down);
                    status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
                }
                else if(drifter.input.MoveX != 0){
                    SetHitboxesActive(false);
                    drifter.animator.Play("W_Side");
                    SetupAttackID(DrifterAttackType.Aerial_Q_Side);
                    status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
                }
                else {
                    SetHitboxesActive(false);
                    drifter.animator.Play("W_Neutral");
                    SetupAttackID(DrifterAttackType.Aerial_Q_Neutral);
                    status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4f); 
                }
            }
        }
    }

    public void resetRecovery(){
        currentRecoveries = maxRecoveries;
    }

    public void Hit(DrifterAttackType attackType, int attackID, GameObject target)
    {
        UnityEngine.Debug.Log("HIT DETECTED IN PLAYER ATTACKS");
    }

    public void SetupAttackID(DrifterAttackType attackType)
    {
        AttackType = attackType;
        AttackID = NextID;
        Debug.Log("in here " + AttackType);
        Debug.Log("in here 2 " + AttackID);
        foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.GetComponent<Collider2D>().enabled = false;
            Debug.Log("1");
            hitbox.AttackID = AttackID;
            Debug.Log("2");
            hitbox.AttackType = AttackType;
            Debug.Log("3");
            hitbox.AttackData = Attacks[AttackType];
            Debug.Log("4");
            hitbox.Active = false;
            Debug.Log("5");
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
