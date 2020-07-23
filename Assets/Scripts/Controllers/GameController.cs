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
    UIController uiController;
    SpawnController spawnController;

    
    // Bout them multipule players
    [Header("Check box if hosting")]
    public bool isHost;
    public NetworkEntityList Entities; // set when game 
    [Header("Don't ship with this.")]
    public string hostIP = "68.187.67.135";
    public int HostID = 18;
    public int playerNumber; // used for UI indexing and other stuff maybe

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
    }

    public void ChooseYerDrifter() {
        //EVANS: HANDSHAKE
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

    public void BeginMatch() {
        // Get player count & choices
        // Create appropriate spawn points
        // Create player characters & give them an input
        // Yeet into world and allow playing the game
        GetComponent<NetworkHost>()?.StartGame();
    }
}