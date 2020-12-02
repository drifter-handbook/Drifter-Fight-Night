// https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/
// https://wiki.unity3d.com/index.php/Toolbox

using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class GameController : MonoBehaviour
{
    public enum VolumeType
    {
        MASTER,
        MUSIC,
        SFX
    };

    //* Serialized members
    [Header("Check box if hosting")]
    public bool IsHost;

    public const int MAX_PLAYERS = 8;

    // Evans's horrid hack, please help me fix this Lyn
    public string selectedStage = null;
    public int winner = -1;

    //* Data storage
    // Character List
    
    public bool IsPaused { get; private set; } = false;

    //* Prefabs
    public GameObject characterCardPrefab;
    public GameObject deathExplosionPrefab;

    //* Variables
    string SceneName { get; set; }

    public static GameController Instance { get; private set; }

    public string Username = "test_user";

    [NonSerialized]
    public NetworkClient client;
    [NonSerialized]
    public MatchmakingClient matchmakingClient;
    [NonSerialized]
    public NetworkHost host;
    [NonSerialized]
    public MatchmakingHost matchmakingHost;

    [NonSerialized]
    public IPEndPoint NatPunchServer = new IPEndPoint(IPAddress.Parse("75.134.27.221"), 6996);
    [NonSerialized]
    public IPEndPoint MatchmakingServer = new IPEndPoint(IPAddress.Parse("75.134.27.221"), 6997);

    public CustomControls controls;

    public int PlayerID = -1;
    public float[] volume = { -1f, -1f, -1f };

    Coroutine endingGame = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
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

    // Only the host gets to see this guy
    public void BeginMatch()
    {
        // Get player count & choices
        // Create appropriate spawn points
        // Create player characters & give them an input
        // Yeet into world and allow playing the game
        host?.SetScene(selectedStage);
    }

    public void EndMatch(float delay)
    {
        // Get player count & choices
        // Create appropriate spawn points
        // Create player characters & give them an input
        // Yeet into world and allow playing the game
        //host?.SetScene("Endgame");
        if(endingGame==null)endingGame=StartCoroutine(EndGameCoroutine(delay));
    }

    IEnumerator EndGameCoroutine(float delay)
    {
        //Save Players in the game before the list is yeeted.
        Dictionary<int,DrifterType> temp = new Dictionary<int,DrifterType>();

        foreach(KeyValuePair<int, GameObject> kvp in NetworkPlayers.Instance.players){
            temp.Add(kvp.Key,kvp.Value.GetComponent<Drifter>().GetDrifterType());
        }
        GameController.Instance.CleanupNetwork();
        yield return new WaitForSeconds(delay);
        yield return SceneManager.LoadSceneAsync("Endgame");

        EndgameImageHandler endHandler = GameObject.FindGameObjectWithTag("EndgamePic").GetComponent<EndgameImageHandler>();

        foreach (KeyValuePair<int, DrifterType> kvp in temp)
        {
            if(kvp.Key == Instance.winner)
            {
                endHandler.playWinnerAudio(kvp.Key==-1?0:kvp.Key);
                endHandler.setWinnerPic(kvp.Value, CharacterMenu.ColorFromEnum[(PlayerColor)(kvp.Key==-1?0:kvp.Key)]);

            }
            else
            {
                endHandler.setSillyImage(kvp.Value, CharacterMenu.ColorFromEnum[(PlayerColor)(kvp.Key==-1?0:kvp.Key)]);
            }
        }

         while (SceneManager.GetActiveScene().name == "Endgame")
        {
            yield return null;
            if (Input.anyKey)
            {
                yield return SceneManager.LoadSceneAsync("MenuScene");
                CleanupNetwork();
                endingGame = null;
                yield break;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            IsPaused = !IsPaused;
        }
       /* if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }*/
    }

    public void UpdateSFXVolume(float val)
    {
        AudioSource source = GetComponent<AudioSource>();
        source.volume = val;
    }

    public void StartNetworkHost()
    {
        host = gameObject.AddComponent<NetworkHost>();
        NetworkSync sync = gameObject.AddComponent<NetworkSync>();
        sync.Initialize(0, "GameController");
        host.Initialize();
        matchmakingHost = GetComponent<MatchmakingHost>() ?? gameObject.AddComponent<MatchmakingHost>();
        PlayerID = 0;
    }
    public void StartNetworkClient(string roomCode)
    {
        client = gameObject.AddComponent<NetworkClient>();
        NetworkSync sync = gameObject.AddComponent<NetworkSync>();
        sync.Initialize(0, "GameController");
        client.Initialize();
        matchmakingClient = GetComponent<MatchmakingClient>() ?? gameObject.AddComponent<MatchmakingClient>();
        matchmakingClient.JoinRoom = roomCode;
    }
    public void CleanupNetwork()
    {
        PlayerID = -1;
        if (IsHost)
        {
            Destroy(host);
            host = null;
            Destroy(matchmakingHost);
            matchmakingHost = null;
        }
        else
        {
            Destroy(client);
            client = null;
            Destroy(matchmakingClient);
            matchmakingClient = null;
        }
        Destroy(GetComponent<NetworkSync>());
        IsHost = false;
    }
}