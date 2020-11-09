using System;
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

    void Awake()
    {
        nextID = 0;
        Entities = new List<GameObject>();
        // add starting entities
        populate();
    }

    public void populate()
    {
        foreach (GameObject obj in StartingEntities)
        {
            AddEntity(obj);
        }
    }

    public GameObject GetEntityPrefab(string type)
    {
        return EntityPrefabs.Find(x => x.name == type);
    }

    public void AddPlayer(int playerID, GameObject player)
    {
        AddEntity(player);
        Players[playerID] = player;
        player.GetComponent<Drifter>().Stocks = 3;
    }

    public void AddEntity(GameObject entity)
    {
        Entities.Add(entity);
    }

    public int remainingPlayers(){
        int numRemaining = 0;
        foreach (GameObject go in Players.Values)
        {
            if(hasStocks(go))numRemaining++;
        }
        return numRemaining;

    }

    public bool hasStocks(GameObject character)
    {
        if (character == null)
        {
            return false;
        }
        if (character.GetComponent<Drifter>().Stocks > 0)
        {
            return true;
        }
        return false;
    }
}
