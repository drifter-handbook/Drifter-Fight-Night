using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "AllPlayerData", menuName = "VirtuaDrifter/AllPlayerData", order = 53)]
public class AllPlayerData : ScriptableObject
{
    [Serializable]
    public class PlayerAttacks
    {
        public string name;
        public DrifterData stats;
        public PlayerAttackData attacks;
    }
    public List<PlayerAttacks> Players = new List<PlayerAttacks>();
    Dictionary<string, DrifterData> DataMap = new Dictionary<string, DrifterData>();
    Dictionary<string, PlayerAttackData> AttackMap = new Dictionary<string, PlayerAttackData>();

    void OnEnable()
    {
        foreach (PlayerAttacks pa in Players)
        {
            DataMap[pa.name] = pa.stats;
            AttackMap[pa.name] = pa.attacks;
        }
    }

    public DrifterData GetStats(string name)
    {
        return DataMap[name];
    }
    public PlayerAttackData GetAttacks(string name)
    {
        return AttackMap[name];
    }
}
