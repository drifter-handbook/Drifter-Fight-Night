// attack types
using System;
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

// collection of data on all attacks a player has
[CreateAssetMenu(fileName = "AttackData", menuName = "VirtuaDrifter/AttackData", order = 69)]
public class DrifterAttackData : ScriptableObject
{
    [Serializable]
    public class SingleAttack
    {
        public DrifterAttackType attack;
        public SingleAttackData attackData;
    }
    public List<SingleAttack> Attacks = new List<SingleAttack>();
    Dictionary<DrifterAttackType, SingleAttackData> AttackMap = new Dictionary<DrifterAttackType, SingleAttackData>();

    void OnEnable()
    {
        foreach (SingleAttack sa in Attacks)
        {
            AttackMap[sa.attack] = sa.attackData;
        }
    }

    public SingleAttackData this[DrifterAttackType attackType]
    {
        get { return AttackMap[attackType]; }
    }
}