using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AllDrifterData
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

    public void FinishLoading()
    {
        foreach (DrifterAttacks pa in Players)
        {
            DataMap[pa.name] = pa.stats;
            DataMap[pa.name].FinishLoading();
            AttackMap[pa.name] = pa.attacks;
            AttackMap[pa.name].FinishLoading();
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
