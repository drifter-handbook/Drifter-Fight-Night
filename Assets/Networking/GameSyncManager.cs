using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
public class GameSyncManager : MonoBehaviour
{
    // set when game starts
    public NetworkEntityList Entities;

    // objects to sync
    [Header("Check box if hosting")]
    public bool IsHost = false;

    public string HostIP = "68.187.67.135";
    public int HostID = 18;

    void Awake()
    {
        // if we are host
        if (IsHost)
        {
            gameObject.AddComponent<NetworkHost>();
        }
        else
        {
            NetworkClient client = gameObject.AddComponent<NetworkClient>();
            client.Host = HostIP;
            client.HostID = HostID;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
