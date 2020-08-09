// https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/
// https://wiki.unity3d.com/index.php/Toolbox

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct ConnectedPlayer
{
    // unique, find player by Entities.Players[playerID]
    public int PlayerID { get; set; }
    public bool IsHost { get; set; }
    // index in game instance
    public int PlayerIndex { get; set; }
}

[DisallowMultipleComponent]
public class GameController : MonoBehaviour
{
    //* Serialized members
    [Header("Check box if hosting")]
    public bool IsHost;
    [Header("Don't ship with this.")]
    public string hostIP = "68.187.67.135";
    public int HostID = 18;

    public const int MAX_PLAYERS = 8;

    //* Data storage
    public AllDrifterData AllData; // Character List
    // Source of Truth on CharSel:
    public List<CharacterSelectState> CharacterSelectStates = new List<CharacterSelectState>() { };
    // It's you!
    public ConnectedPlayer LocalPlayer {
        get
        {
            ConnectedPlayer c = new ConnectedPlayer();
            c.IsHost = IsHost;
            NetworkID id = GetComponent<NetworkID>();
            c.PlayerID = id == null ? -1 : id.PlayerID;
            CharacterSelectState charSelState = CharacterSelectStates.Find(x => x.PlayerID == c.PlayerID);
            c.PlayerIndex = charSelState == null ? -1 : charSelState.PlayerIndex;
            return c;
        }
    }
    public NetworkEntityList Entities; // Holds all entities, incl players!!!
    public bool IsPaused { get; private set; } = false;

    //* Prefabs
    public GameObject characterCardPrefab;
    public GameObject deathExplosionPrefab;

    //* Variables
    string SceneName { get; set; }

    //* This is a singleton (& the only singleton)
    protected GameController() { } // Get instance with GameController.Instance
    private static GameController instance;
    public static GameController Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
        PreLoad();
    }

    void PreLoad()
    {
        // Do something!
        BeginHandshake();
    }

    public void Load(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName) return;
        SceneManager.LoadScene(sceneName);
        SceneName = sceneName;
    }

    // use if SceneName is not synced
    public string GetSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public void ChooseYerDrifter()
    {
        BeginHandshake();
        Load("CharacterSelect");
    }

    void BeginHandshake()
    { // I almost named this IWantAGoodCleanFight and you should be thankful I didn't
        if (IsHost)
        {
            gameObject.AddComponent<NetworkHost>();
        }
        else
        {
            NetworkClient client = gameObject.AddComponent<NetworkClient>();
            client.Host = hostIP;
            client.HostID = HostID;
        }
    }

    // Only the host gets to see this guy
    public void BeginMatch()
    {
        // Get player count & choices
        // Create appropriate spawn points
        // Create player characters & give them an input
        // Yeet into world and allow playing the game
        GetComponent<NetworkHost>()?.StartGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GetComponent<NetworkHost>()?.StartGame();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            IsPaused = !IsPaused;
        }
    }
}