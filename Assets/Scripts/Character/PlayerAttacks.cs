﻿using System;
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
    public bool hasAirVariant;
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
    public static int NextID { get { nextID++;if(nextID>100)nextID=0; return nextID; } }

    // current attack ID and Type, used for outgoing attacks
    public int AttackID { get; private set; }
    public DrifterAttackType AttackType { get; private set; }
    public List<SingleAttack> AttackMap = new List<SingleAttack>();
    public Dictionary<DrifterAttackType,SingleAttackData> Attacks = new Dictionary<DrifterAttackType,SingleAttackData>();
    public Dictionary<DrifterAttackType,bool> AttackVariants = new Dictionary<DrifterAttackType,bool>();
   
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
            AttackVariants[attack.attack] = attack.hasAirVariant;

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
            return;
    

        bool canAct = !status.HasStunEffect() && !drifter.guarding && !ledgeHanging && !status.HasStatusEffect(PlayerStatusEffect.STANCE);
        bool canSpecial = !status.HasStunEffect() && !ledgeHanging;

        if((movement.grounded && !status.HasStatusEffect(PlayerStatusEffect.END_LAG)) || status.HasEnemyStunEffect()) resetRecovery();
        
        if(superPressed())  movement.superCancel();
        
        else if (grabPressed() && canAct) useGrab();

        else if(specialPressed() && canSpecial) useSpecial();
        
        else if (lightPressed() && canAct) useNormal();

    }

    public void useSpecial()
    {
        movement.canLandingCancel = false;
        if(drifter.input[0].MoveY > 0 && currentRecoveries >0)
            {
                StartAttack(DrifterAttackType.W_Up);
                currentRecoveries--;
            }
            else if((!W_Down_Is_Recovery || currentRecoveries >0) && drifter.input[0].MoveY < 0)
            {
                StartAttack(DrifterAttackType.W_Down);
                if(W_Down_Is_Recovery)currentRecoveries--;
            }
            else if((!W_Side_Is_Recovery || currentRecoveries >0) &&drifter.input[0].MoveX!=0)
            {
                StartAttack(DrifterAttackType.W_Side);
                if(W_Side_Is_Recovery)currentRecoveries--;
            }
            else if((!W_Neutral_Is_Recovery || currentRecoveries >0) &&drifter.input[0].MoveY==0 && drifter.input[0].MoveX==0)
            {
                StartAttack(DrifterAttackType.W_Neutral);
                if(W_Neutral_Is_Recovery)currentRecoveries--;
                
            }
    }

    public void useNormal()
    {
        drifter.canSpecialCancelFlag = true;

        if (movement.grounded)
        {
            if(drifter.input[0].MoveY > 0)StartAttack(DrifterAttackType.Ground_Q_Up);
            else if(drifter.input[0].MoveY < 0)StartAttack(DrifterAttackType.Ground_Q_Down);
            else if(drifter.input[0].MoveX!=0)StartAttack(DrifterAttackType.Ground_Q_Side);
            else StartAttack(DrifterAttackType.Ground_Q_Neutral);
        }
        else
        {   
            movement.canLandingCancel = true;    
            if(drifter.input[0].MoveY > 0)StartAttack(DrifterAttackType.Aerial_Q_Up);
            else if(drifter.input[0].MoveY < 0)StartAttack(DrifterAttackType.Aerial_Q_Down);
            else if(drifter.input[0].MoveX!=0)StartAttack(DrifterAttackType.Aerial_Q_Side);
            else StartAttack(DrifterAttackType.Aerial_Q_Neutral);
        }
    }

    public void useGrab()
    {
        if (movement.grounded)StartAttack(DrifterAttackType.E_Side);
        else
        {
            movement.canLandingCancel = true;
            StartAttack(DrifterAttackType.E_Air);  
        } 
    }

    public bool lightPressed()
    {
        bool _lightPressed = false;
        for (int i = 0; i < drifter.input.Length - 3; i++)
            if(!_lightPressed) _lightPressed = !drifter.input[i+2].Light && drifter.input[i+1].Light && drifter.input[i].Light;
            else return _lightPressed;
        return _lightPressed;
    }
    public bool specialPressed()
    {
        bool _specialPressed = false;
        for (int i = 0; i < drifter.input.Length - 3; i++)
           if(!_specialPressed) _specialPressed = drifter.input[2].Special &&  drifter.input[1].Special && drifter.input[0].Special;
           else return _specialPressed;

        return _specialPressed;
    }
    public bool grabPressed()
    {
        bool _grabPressed = false;
        for (int i = 0; i < drifter.input.Length - 3; i++)
        {
            if(!_grabPressed) _grabPressed = ((drifter.input[i].Light || drifter.input[i + 1].Light) && !drifter.input[i + 2].Light &&
                    (drifter.input[i].Special || drifter.input[i + 1].Special) && !drifter.input[i+ 2].Special)
                    || (!drifter.input[i + 2].Grab && drifter.input[i + 1].Grab && drifter.input[i].Grab);
            else return _grabPressed;
        }
        return _grabPressed;
    }

    public bool superPressed()
    {
        bool _superPressed = false;
        for (int i = 0; i < drifter.input.Length - 3; i++)
            if(!_superPressed) _superPressed = !drifter.input[i+2].Super && drifter.input[i+1].Super && drifter.input[i].Super;
            else return _superPressed;

        return _superPressed;
    }

    public void resetRecovery(){
        currentRecoveries = maxRecoveries;
    }

    public void StartAttack(DrifterAttackType attackType)
    {
        drifter.gainSuperMeter(.05f);
        
        SetHitboxesActive(false);
        status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,8f);
        if(!AttackVariants[attackType])
            drifter.PlayAnimation(AnimatorStates[attackType]);
        else if(movement.grounded)
            drifter.PlayAnimation(AnimatorStates[attackType] + "_Ground");
        else
            drifter.PlayAnimation(AnimatorStates[attackType] + "_Air");
        SetupAttackID(attackType);
        //UnityEngine.Debug.Log("STARTING ATTACK: " + attackType.ToString() + "  With attackID: " + AttackID);


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
