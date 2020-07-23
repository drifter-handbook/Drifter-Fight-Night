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
    public bool isHost;
    public string hostIP;

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

    public void BeginMatch() {
        // Get player count & choices
        // Create appropriate spawn points
        // Create player characters & give them an input
        // Yeet into world and allow playing the game
        GetComponent<NetworkHost>()?.StartGame();
    }
}