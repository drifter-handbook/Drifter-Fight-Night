// collection of data on a single attack
using System.Collections.Generic;
using UnityEngine;

public enum HitSpark
{
    NONE, POKE, BASH, PIERCE, GRAB, GUARD_STRONG, GUARD_WEAK, SPIKE, MAGICWEAK, CRIT, MAGICSTRONG, OOMPHSPARK, LUCILLE, REFLECT, STAR, STAR_FAST, OOMPHDARK, HEAL, RING, STAR1, STAR2
}


[CreateAssetMenu(fileName = "SingleAttackData", menuName = "VirtuaDrifter/SingleAttackData", order = 70)]
public class SingleAttackData : ScriptableObject
{
    public float AttackDamage = 10.0f;
    public float Knockback = 10.0f;
    public float KnockbackScale = .5f;
    public float HitStun = -1f;
    //public float EndLag = 0.1f;
    public bool isGrab = false;
    public float AngleOfImpact = 45f;
    public PlayerStatusEffect StatusEffect = PlayerStatusEffect.HIT;
    public float StatusDuration =.1f;
    public HitSpark HitVisual = HitSpark.POKE;
}
