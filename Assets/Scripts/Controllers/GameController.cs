// https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/
// https://wiki.unity3d.com/index.php/Toolbox

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameController : Singleton<GameController>
{
    public AllPlayerData PlayerData; 

    // Prevent instantiation - get instance with GameController.Instance
    protected GameController() {}

    // Tree of controllers
    public UIController uiController {get; private set;}
    public SpawnController spawnController {get; private set;}

    // Useful prefabs
    public GameObject characterCardPrefab;
    
    // Bout them multipule players
    [Header("Check box if hosting")]
    public bool isHost;
    public NetworkEntityList Entities; // set when game 
    [Header("Don't ship with this.")]
    public string hostIP = "68.187.67.135";
    public int HostID = 18;
    public int playerNumber; // used for UI indexing and other stuff maybe

    public string SceneName {get; protected set;}

    private void Awake() {
        DontDestroyOnLoad(this.gameObject);
        PreLoad();
    }

    void PreLoad() {
        uiController = gameObject.AddComponent<UIController>();
        spawnController = gameObject.AddComponent<SpawnController>();
    }

    public void Load(string sceneName) {
        if (SceneManager.GetActiveScene().name == sceneName) return;
        SceneManager.LoadScene(sceneName);
        SceneName = sceneName;
    }

    // use if SceneName is desynced
    public string GetSceneName() {
        return SceneManager.GetActiveScene().name;
    }

    public void ChooseYerDrifter() {
        BeginHandshake();
        Load("CharacterSelect");
    }

    void BeginHandshake(){ // I almost named this IWantAGoodCleanFight and you should be thankful I didn't
        // if we are host
        if (isHost)
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
    public void BeginMatch() {
        // Get player count & choices
        // Create appropriate spawn points
        // Create player characters & give them an input
        // Yeet into world and allow playing the game
        GetComponent<NetworkHost>()?.StartGame();
    }
}