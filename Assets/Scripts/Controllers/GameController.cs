// https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/
// https://wiki.unity3d.com/index.php/Toolbox

using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using GameAnalyticsSDK;

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

    public const int MAX_PLAYERS = 8;

    // Evans's horrid hack, please help me fix this Lyn
    public string selectedStage = null;
    public int[] winnerOrder;

    //* Data storage
    // Character List

    public bool IsPaused { get; set; } = false;

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
    public IPEndPoint NatPunchServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6996);
    [NonSerialized]
    public IPEndPoint MatchmakingServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6997);


    public InputActionAsset[] baseControls;

    

    //[NonSerialized]
    public InputActionAsset[] availableControls;

    //[NonSerialized]
    public InputActionAsset[] controls;

    public int PlayerID = -1;
    public float[] volume = { -1f, -1f, -1f };

    Coroutine endingGame = null;
    string cachedRoomCode ="";

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

    void Start() {
        GameAnalytics.Initialize();
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "startGame");
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

        AssignInputAssest();
        
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
   
        yield return new WaitForSeconds(delay);

        host?.SetScene("Endgame");
        endingGame = null;
        yield break;

    }

    void Update()
    {
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


    //Associates each input asset with a controller
    //Call when a new controller is plugged in to associate it with an asset.
    //Populates the keyboard as the default main control system.
    public void AssignInputAssest()
    {

        controls = new InputActionAsset[1];

        availableControls = new InputActionAsset[Gamepad.all.Count+1];

        availableControls[0] = baseControls[0];
        controls[0] = baseControls[0];

        //Get all connected controllers on startup
        for(int i = 0; i < Gamepad.all.Count; i++)
        {

            //Create a new input action asset
            InputActionAsset controller = new InputActionAsset();

            //Janky clone operation beacue the real clone doesnt work?
            controller.LoadFromJson(baseControls[1].ToJson());

            //Player x is assigned this control scheme
            //Make this run off of peer id?
            availableControls[i+1] = controller;

            //If there is a controller, use it
            //Change this to make a new array on use
            if(Gamepad.all.Count > i)
                availableControls[i+1].devices = new Gamepad[] {Gamepad.all[i]};
            else
                availableControls[i+1].devices = new Gamepad[] {};
        }

    }


    public int checkForNewControllers()
    {
        int addedCount = 0;
        foreach(InputActionAsset controller in GameController.Instance.availableControls)
        {
            if(controller != null && controller.FindAction("Pause").triggered && !Array.Exists(controls, element => element == controller))
            {
                addController(controller);
                addedCount++;
            }
        }

        return addedCount;
    }


    public int checkForRemoveControllers()
    {
        int removedIndex = -1;
        foreach(InputActionAsset controller in GameController.Instance.controls)
        {
            if(controller != null && controller.FindAction("Quit").triggered)
            {
                removedIndex = removeControler(controller);
                UnityEngine.Debug.Log("REMOVED: " + controller);
            }
        }

        return removedIndex;

    }

    private void addController(InputActionAsset controller)
    {
        InputActionAsset[] tempControls = new InputActionAsset[controls.Length +1];
        for(int i = 0; i < controls.Length; i++)
        {
            if(controls[i] != null)tempControls[i] = controls[i];
        }

        tempControls[controls.Length] = controller;
        controls = tempControls;

        UnityEngine.Debug.Log(controls[controls.Length -1]);
    }

    private int removeControler(InputActionAsset controller)
    {
        InputActionAsset[] tempControls = new InputActionAsset[controls.Length -1];

        int index = -1;

        for(int i = 0; i < controls.Length; i++)
        {
            if(controls[i] != controller)tempControls[i] = controls[i];
            else index = i;
        }

        controls = tempControls;
        return index;
    }


}
