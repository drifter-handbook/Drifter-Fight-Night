// attack types
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum PlayerAttackType
{
    Null,
    Ground_Q_Side, Ground_Q_Down, Ground_Q_Up, Ground_Q_Neutral,
    Aerial_Q_Side, Aerial_Q_Down, Aerial_Q_Up, Aerial_Q_Neutral,
    W_Side, W_Down, W_Up, W_Neutral,
    E_Side, Aerial_E_Down, E_Up, E_Neutral
}

// collection of data on all attacks a player has
[CreateAssetMenu(fileName = "AttackData", menuName = "VirtuaDrifter/AttackData", order = 69)]
public class PlayerAttackData : ScriptableObject
{
    [Serializable]
    public class SingleAttack
    {
        public PlayerAttackType attack;
        public SingleAttackData attackData;
    }
    public List<SingleAttack> Attacks = new List<SingleAttack>();
    Dictionary<PlayerAttackType, SingleAttackData> AttackMap = new Dictionary<PlayerAttackType, SingleAttackData>();

    void OnEnable()
    {
        foreach (SingleAttack sa in Attacks)
        {
            AttackMap[sa.attack] = sa.attackData;
        }
    }

    public SingleAttackData this[PlayerAttackType attackType]
    {
        get { return AttackMap[attackType]; }
    }
}