using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum DrifterType
{
    None,
    Bojo,
    Swordfrog,
    Lady_Parhelion,
    Spacejam,
    Orro,
    Ryyke,
    Megurin,
    Nero,
}

/** 
 * This is the class that will be put into a prefab and instantiated 
 */
[RequireComponent(typeof(PlayerSync))]
[RequireComponent(typeof(PlayerMovement))]
public class Drifter : MonoBehaviour
{
    public DrifterData drifterData;
    public PlayerMovement movement;
    public PlayerSync sync;
    public CustomControls controls;

    public Animator animator;
    public PlayerAnimatorState animatorState { get; set; } = new PlayerAnimatorState();

    public int Stocks = 0;
    public float DamageTaken = 0f;
    public int Charge = 0;

    public float BlockReduction = .5f;

    public PlayerInputData input { get; set; } = new PlayerInputData();
    public PlayerInputData prevInput = new PlayerInputData();
    
    public Color myColor;

    void Awake()
    {
    }

    void LateUpdate()
    {
        // set previous frame player input
        prevInput.CopyFrom(input);
    }

    public void SetColor(Color color)
    {
        myColor = color;
        transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().color = color;
    }
    public Color GetColor()
    {
        return transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().color;
    }

    // used by clients
    public void SyncAnimatorState(PlayerAnimatorState state)
    {
        int ID = GetComponent<INetworkSync>().ID;
        animator.SetBool("Grounded", state.Grounded);
        animator.SetBool("Walking", state.Walking);
        animator.SetBool("Guarding", state.Guarding);
        animator.SetBool("Empowered", state.Empowered);
        animator.SetBool("HasCharge", state.HasCharge);
        animator.SetBool("HitStun", state.HitStun);
        if (state.Attack) animator.SetTrigger("Attack");
        if (state.Grab) animator.SetTrigger("Grab");
        if (state.Jump) animator.SetTrigger("Jump");
        if (state.Recovery) animator.SetTrigger("Recovery");
        if (state.Aerial) animator.SetTrigger("Aerial");
        if (state.W_Side) animator.SetTrigger("W_Side");
        if (state.W_Neutral) animator.SetTrigger("W_Neutral");
        if (state.W_Down) animator.SetTrigger("W_Down");
        if (state.Roll) animator.SetTrigger("Roll");
    }
    // used by host
    public void SetAnimatorTrigger(string s)
    {
        if (GameController.Instance.IsHost)
        {
            animator.SetTrigger(s);
        }
        switch (s)
        {
            case "Attack":
                animatorState.Attack = true;
                break;
            case "Grab":
                animatorState.Grab = true;
                break;
            case "Jump":
                animatorState.Jump = true;
                break;
            case "Recovery":
                animatorState.Recovery = true;
                break;
            case "Aerial":
                animatorState.Aerial = true;
                break;
            case "W_Side":
                animatorState.W_Side = true;
                break;
            case "W_Neutral":
                animatorState.W_Neutral = true;
                break;
            case "W_Down":
                animatorState.W_Down = true;
                break;
            case "Roll":
                animatorState.Roll = true;
                break;
        }
    }
    public void ResetAnimatorTriggers()
    {
        animatorState.Attack = false;
        animatorState.Grab = false;
        animatorState.Jump = false;
        animatorState.Recovery = false;
        animatorState.Aerial = false;
        animatorState.W_Neutral = false;
        animatorState.W_Side = false;
        animatorState.W_Down = false;
        animatorState.Roll = false;
        animatorState.HasCharge = false;
        animatorState.HitStun = false;

    }
    public void SetAnimatorBool(string s, bool value)
    {
        if (GameController.Instance.IsHost)
        {
            animator.SetBool(s, value);
        }
        switch (s)
        {
            case "Grounded":
                animatorState.Grounded = value;
                break;
            case "Walking":
                animatorState.Walking = value;
                break;
            case "Guarding":
                animatorState.Guarding = value;
                break;
            case "Empowered":
                animatorState.Empowered = value;
                break;
            case "HasCharge":
                animatorState.HasCharge = value;
                break;
            case "HitStun":
                animatorState.HitStun = value;
                break;                 
        }
    }
}

public class PlayerAnimatorState : ICloneable
{
    public bool Grounded = false;
    public bool Walking = false;
    public bool Guarding = false;
    public bool Empowered = false;
    public bool Attack = false;
    public bool Grab = false;
    public bool Jump = false;
    public bool Recovery = false;
    public bool Aerial = false;
    public bool W_Side = false;
    public bool W_Down = false;
    public bool W_Neutral = false;
    public bool Roll = false;
    public bool HasCharge = false;
    public bool HitStun = false;

    public object Clone()
    {
        return new PlayerAnimatorState()
        {
            Grounded = Grounded,
            Walking = Walking,
            Guarding = Guarding,
            Empowered = Empowered,
            Attack = Attack,
            Grab = Grab,
            Jump = Jump,
            Recovery = Recovery,
            Aerial = Aerial,
            W_Side = W_Side,
            W_Down = W_Down,
            W_Neutral = W_Neutral,
            Roll = Roll,
            HasCharge = HasCharge,
            HitStun = HitStun
        };
    }
}