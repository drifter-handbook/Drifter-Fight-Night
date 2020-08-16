// collection of data on a single attack
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SingleAttackData", menuName = "VirtuaDrifter/SingleAttackData", order = 70)]
public class SingleAttackData : ScriptableObject
{
    public float AttackDamage = 10.0f;
    public float Knockback = 10.0f;
    public float KnockbackScale = 1.0f;
    public float HitStun = 0.1f;
    public float EndLag = 0.1f;
    public float AngleOfImpact = 45f;
    public PlayerStatusEffect StatusEffect;
    public float StatusDuration =.5f;

}
