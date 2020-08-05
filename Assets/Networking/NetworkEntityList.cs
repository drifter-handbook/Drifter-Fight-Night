﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEntityList : MonoBehaviour
{
    public List<GameObject> EntityPrefabs = new List<GameObject>();

    public List<GameObject> SpawnPoints = new List<GameObject>();

    public List<GameObject> StartingEntities = new List<GameObject>();

    [NonSerialized]
    public List<GameObject> Entities = new List<GameObject>();

    // map from PlayerID -> GameObject
    [NonSerialized]
    public Dictionary<int, GameObject> Players = new Dictionary<int, GameObject>();

    static int nextID = 0;
    public static int NextID { get { nextID++; return nextID; } }

    //Noah's Stock Garbage
    // map from PlayerObject -> Stock Count
    public Dictionary<GameObject, int> Stocks = new Dictionary<GameObject, int>();

    void Awake()
    {
        nextID = 0;
        // add starting entities
        foreach (GameObject obj in StartingEntities)
        {
            AddEntity(obj);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject GetEntityPrefab(string type)
    {
        return EntityPrefabs.Find(x => x.name == type);
    }

    public void AddPlayer(int playerID, GameObject player)
    {
        AddEntity(player);
        Players[playerID] = player;
        Stocks[player] = 3;
    }

    public void AddEntity(GameObject entity)
    {
        Entities.Add(entity);
    }

    public bool hasStocks(GameObject character)
    {
        if (character == null || !Stocks.ContainsKey(character))
        {
            return false;
        }
        if (Stocks[character] > 0) {
            return true;
        }
        return false;
    }
}
