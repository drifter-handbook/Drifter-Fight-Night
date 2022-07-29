// https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/
// https://wiki.unity3d.com/index.php/Toolbox

using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem;
using GameAnalyticsSDK;
using UnityEngine.EventSystems;

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
    public bool IsOnline;
    public bool IsTraining;

    //public const int MAX_PLAYERS = 4;

    // Evans's horrid hack, please help me fix this Lyn
    public string selectedStage = null;
    public int[] winnerOrder;

    //* Data storage
    // Character List

    private bool _IsPaused = false;

    public bool canPause = false;

    public bool IsPaused
    {
        get { return _IsPaused;}
        set {
            _IsPaused = value;

            toggleInuptSystem(value);

            Time.timeScale = _IsPaused?0f:1f;
        }
    }
    public int maxPlayerCount
    {
        get{ return inputManager.maxPlayerCount;}
    }

    //TODO move this elsewhere
    //* Prefabs
    public GameObject characterCardPrefab;

    //public GameObject escapeMenu;

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
    public IPEndPoint NatPunchServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6996);
    [NonSerialized]
    public IPEndPoint MatchmakingServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6997);

    [NonSerialized]
    public PlayerInputManager inputManager;

    //[NonSerialized]
    public Dictionary<int,PlayerInput> controls = new Dictionary<int,PlayerInput>();

    public int PlayerID = -1;
    public float[] volume = { -1f, -1f, -1f };

    Coroutine endingGame = null;
    string cachedRoomCode ="";
    bool clearingPeers = false;



    public List<GameObject> NetworkTypePrefabs = new List<GameObject>();

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
        inputManager = GetComponent<PlayerInputManager>();
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        //GameAnalytics.Initialize();
        //GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "startGame");
        // this is horrid practice please dont do this but
        if(IsOnline)
        {
            string server = Resources.Load<TextAsset>("Config/server_ip").text;
            if (IPAddress.TryParse(server, out IPAddress address))
            {
            }
            else
            {
                IPHostEntry host = Dns.GetHostEntry(server);
                address = host.AddressList[0];
            }
            NatPunchServer = new IPEndPoint(address, NatPunchServer.Port);
            MatchmakingServer = new IPEndPoint(address, MatchmakingServer.Port);
        }

        aggregatePrefabs("Assets/Resources/");

        //AssignInputAssest();
        
    }

    public void addUser(PlayerInput playerInput)
    {
        int peerID = -1;
        while(controls.ContainsKey(peerID))
            peerID++;
        controls.Add(peerID,playerInput);

        FindObjectOfType<CharacterMenu>()?.AddCharSelState(peerID);

        playerInput.ActivateInput();

        DontDestroyOnLoad(playerInput);

        if(controls.Count >= 1 && IsTraining)
            DisableJoining();

    }

    public void removeUserByPeer(int peerID)
    {
        if(!controls.ContainsKey(peerID))
        {
            UnityEngine.Debug.Log("PEER ID " + peerID +" ATTEMPTED TO BE REMOVED BUT WAS NOT FOUND");
            return;
        }
        
        controls[peerID].DeactivateInput();
        //inputManager.Un
        Destroy(controls[peerID].gameObject);
        controls.Remove(peerID);
        FindObjectOfType<CharacterMenu>()?.RemoveCharSelState(peerID);
        if(peerID != -1) host.Peers.Remove(peerID);
        if(!clearingPeers && IsTraining && controls.Count ==0)
            EnableJoining();
    }

    public void removeAllPeers()
    {
        clearingPeers = true;
        DisableJoining();
        List<int> peersToRemove = new List<int>();
        foreach(int peerID in controls.Keys)
            peersToRemove.Add(peerID);

        foreach(int peer in peersToRemove)
            removeUserByPeer(peer);
        clearingPeers = false;
    }

    //Wrap enable method
    public void EnableJoining()
    {
        inputManager.EnableJoining();
    }
    //Wrap Disable method
    public void DisableJoining()
    {
        inputManager.DisableJoining();
    }

    public bool CanJoin()
    {
        return inputManager.joiningEnabled;
    }

    public void toggleInuptSystem(bool ui)
    {
        List<int> inputsToToggle = new List<int>();
        foreach(int peerID in controls.Keys)
            inputsToToggle.Add(peerID);

        foreach(int peer in inputsToToggle)
            controls[peer].gameObject.SetActive(!ui);
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
        canPause = true;
        host?.SetScene("Combat");
    }

    public void EndMatch(float delay)
    {
        // Get player count & choices
        // Create appropriate spawn points
        // Create player characters & give them an input
        // Yeet into world and allow playing the game
        //host?.SetScene("Endgame");
        canPause = false;

        if(endingGame==null)endingGame=StartCoroutine(EndGameCoroutine(delay));
    }

    IEnumerator EndGameCoroutine(float delay)
    {
        removeAllPeers();
        yield return new WaitForSeconds(delay);

        host?.SetScene("Endgame");
        endingGame = null;
        yield break;

    }

    public void UpdateSFXVolume(float val)
    {
        AudioSource source = GetComponent<AudioSource>();
        source.volume = val;
    }

    public void StartNetworkHost()
    {
        if(host != null)
        {
            UnityEngine.Debug.Log("STONKY");
            Destroy(GetComponent<NetworkSync>());
            Destroy(GetComponent<NetworkHost>());
            Destroy(host);
        }

        host = gameObject.AddComponent<NetworkHost>();
        NetworkSync sync = gameObject.AddComponent<NetworkSync>();
        sync.Initialize(0, "GameController");
        host.Initialize();
        matchmakingHost = GetComponent<MatchmakingHost>() ?? gameObject.AddComponent<MatchmakingHost>();
        PlayerID = 0;
    }

    public void StartNetworkClient(string roomCode)
    {
        if(client != null)
        {
            UnityEngine.Debug.Log("STINKY");
            Destroy(GetComponent<NetworkSync>());
            Destroy(GetComponent<NetworkClient>());
            Destroy(client);
        }
        cachedRoomCode = roomCode;
        client = gameObject.AddComponent<NetworkClient>();
        NetworkSync sync = gameObject.AddComponent<NetworkSync>();
        sync.Initialize(0, "GameController");
        client.Initialize();
        matchmakingClient = GetComponent<MatchmakingClient>() ?? gameObject.AddComponent<MatchmakingClient>();
        matchmakingClient.JoinRoom = roomCode;
    }

    public void StartNetworkClient()
    {
        StartNetworkClient(cachedRoomCode);
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


        //TODO Remove this?
        //AssignInputAssest();
       
        IsHost = false;
        IsOnline = false;
        endingGame = null;
    }

    //Populates the Network Prefabs list in Lucille Johnson
    private void aggregatePrefabs(string basePath)
    {

        //string[] networkPrefabs = Directory.GetFiles(basePath,"*.prefab",SearchOption.AllDirectories);

        UnityEngine.Object[] networkPrefabs = Resources.LoadAll("", typeof(GameObject));

        for(int i = 0; i < networkPrefabs.Length; i++)

           NetworkTypePrefabs.Add((GameObject)networkPrefabs[i]);

        UnityEngine.Debug.Log("Added " + NetworkTypePrefabs.Count + " Prefabs to the Network Prefab List");

    }

    public GameObject CreatePrefab(string networkType)
    {
        GameObject obj = Instantiate(NetworkTypePrefabs.Find(x => x.name == networkType));
        return obj;
    }

    public GameObject CreatePrefab(string networkType, Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(NetworkTypePrefabs.Find(x => x.name == networkType),position,rotation);
        
        return obj;
    }
}
