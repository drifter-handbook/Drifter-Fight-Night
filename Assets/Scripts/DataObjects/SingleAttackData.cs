// collection of data on a single attack
using System.Collections.Generic;
using UnityEngine;

public enum HitSpark
{
    NONE, POKE, BASH, PIERCE, GRAB, GUARD_STRONG, GUARD_WEAK, SPIKE, MAGICWEAK, CRIT, MAGICSTRONG, OOMPHSPARK, LUCILLE, REFLECT, STAR, STAR_FAST, OOMPHDARK, HEAL, RING, STAR1, STAR2, ORRO_SWEET, DEFAULT_IMPACT, DEFAULT_FLYOUT_PRIMARY, DEFAULT_FLYOUT_SECONDARY, DEFAULT_FLYOUT_TERTIARY
}

public enum HitType
{
    NORMAL, GUARD_CRUSH, GRAB,TRANSCENDANT,BURST
}


[CreateAssetMenu(fileName = "SingleAttackData", menuName = "VirtuaDrifter/SingleAttackData", order = 70)]



public class SingleAttackData : ScriptableObject
{
    #if UNITY_EDITOR
    [Help("All times are in frames. One game frame is .08333 seconds, or 12 frames/second.", UnityEditor.MessageType.Info)]
    #endif
    public float AttackDamage = 10.0f;
    public float Knockback = 10.0f;
    public float KnockbackScale = .5f;
    //A value of -1 uses NO baseline hitstun
    #if UNITY_EDITOR
    [Help("Use +/-x to determine advantage on hit or on shield.", UnityEditor.MessageType.Info)]
    #endif
    public int HitStun = 0;
    public int ShieldStun = 0;

    #if UNITY_EDITOR
    [Help("A negative value will cause the move to base hitpause on the hitstun dealt. A positive value indicates the number of effective frames of hitstun to use in hitstop calculations.", UnityEditor.MessageType.Info)]
    #endif
    public float HitStop= -1f;
    //  #if UNITY_EDITOR
    // [Help("Indicates whether or not the duration of the flat hit-stop will scale as a target takes more damage", UnityEditor.MessageType.Info)]
    // #endif
    // public bool ScaleHitStop = true;
    //public float EndLag = 0.1f;
    #if UNITY_EDITOR
    [Help("How does this move interact with shields? Normal is blocked by shields, Grab ignores shields, guard crush applies extra hitstun to shields, Trancendant will always hit not matter what", UnityEditor.MessageType.Info)]
    #endif
    public HitType hitType = HitType.NORMAL;
   
    public bool mirrorKnockback = false;
    public float AngleOfImpact = 45f;
    
    
    public PlayerStatusEffect StatusEffect = PlayerStatusEffect.HIT;
    public float StatusDuration = .1f;
    public AttackFXSystem HitFX = null;

    public bool canHitGrounded = true;
    public bool canHitAerial = true;

    //public bool knockDown = false;
    public bool canHitKnockedDown = false;

    #if UNITY_EDITOR
    [Help("The percentage range in which knockback scaling applies. -1 is unbounded. Scaling is not prorated and will begin from 0 at the floor.", UnityEditor.MessageType.Info)]
    #endif
    public float scalingLowerBound = -1;
    public float scalingUpperBound = -1;
}
