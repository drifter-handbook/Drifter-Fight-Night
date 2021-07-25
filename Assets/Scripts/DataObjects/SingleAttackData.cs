// collection of data on a single attack
using System.Collections.Generic;
using UnityEngine;

public enum HitSpark
{
    NONE, POKE, BASH, PIERCE, GRAB, GUARD_STRONG, GUARD_WEAK, SPIKE, MAGICWEAK, CRIT, MAGICSTRONG, OOMPHSPARK, LUCILLE, REFLECT, STAR, STAR_FAST, OOMPHDARK, HEAL, RING, STAR1, STAR2, ORRO_SWEET, DEFAULT_IMPACT, DEFAULT_FLYOUT_PRIMARY, DEFAULT_FLYOUT_SECONDARY, DEFAULT_FLYOUT_TERTIARY
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
    #if UNITY_EDITOR
    [Help("Indicates whether the move's hitstun scales with the opponents damage. A true value disables scaling.", UnityEditor.MessageType.Info)]
    #endif
    public bool hasStaticHitstun = false;
    //A value of -1 uses NO baseline hitstun
    #if UNITY_EDITOR
    [Help("A negative numbers indicate the move has no minimum HitStun and will always use the calculated value.", UnityEditor.MessageType.Info)]
    #endif
    public float HitStun = -1f;
    //public float EndLag = 0.1f;
    #if UNITY_EDITOR
    [Help("Does this attack Break shields?", UnityEditor.MessageType.Info)]
    #endif
    public bool isGrab = false;
   
    public bool mirrorKnockback = false;
    public float AngleOfImpact = 45f;
    public PlayerStatusEffect StatusEffect = PlayerStatusEffect.HIT;
    public float StatusDuration = .1f;
    public AttackFXSystem HitFX = null;
}
