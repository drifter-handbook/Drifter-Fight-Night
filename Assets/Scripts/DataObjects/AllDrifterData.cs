using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "AllDrifterData", menuName = "VirtuaDrifter/AllDrifterData", order = 53)]
public class AllDrifterData : ScriptableObject
{
    [Serializable]
    public class DrifterAttacks
    {
        public string name;
        public DrifterData stats;
        public DrifterAttackData attacks;
    }
    public List<DrifterAttacks> Players = new List<DrifterAttacks>();
    Dictionary<string, DrifterData> DataMap = new Dictionary<string, DrifterData>();
    Dictionary<string, DrifterAttackData> AttackMap = new Dictionary<string, DrifterAttackData>();

    void OnEnable()
    {
        foreach (DrifterAttacks pa in Players)
        {
            DataMap[pa.name] = pa.stats;
            AttackMap[pa.name] = pa.attacks;
        }
    }

    public DrifterData GetStats(string name)
    {
        return DataMap[name];
    }
    public DrifterAttackData GetAttacks(string name)
    {
        return AttackMap[name];
    }
}
