// https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/
// https://wiki.unity3d.com/index.php/Toolbox

using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameController : Singleton<GameController>
{
    // Prevent instantiation - get instance with GameController.Instance
    protected GameController() {}

    // Tree of controllers
    [SerializeField] UIController uiController;
    [SerializeField] SpawnController spawnController;

    // Control flow
    public enum StateType 
    {
        DEFAULT,        // Fall-back state, should never happen
        APPLICATION_START,
        CONNECTING,     // Connecting to multiplayer
        WAITING,        // Waiting for other player to finish selecting a character
        MATCH_START,    // Initializing game state
        PLAYING,        // In-game
        PAUSED,         // In-game but one player has paused the game
        MATCH_OVER,     // Match has ended
    };

    StateType state {get; set;}

    private void Awake() {
        PreLoad();
    }

    void PreLoad() {
        
    }

    public void Pause(bool paused) {

    }

    public void Connect() { // Evans

    }

    public void ChooseYerDrifter() {
        
    }

    public void BeginMatch() { // Lyn
        // Get player count & choices
        // Create appropriate spawn points
        // Create player characters & give them an input
        // Yeet into world and allow playing the game
    }

    public void ShowMenu() {

    }

    public void ShowOptions() {
    }
}