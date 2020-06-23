// https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/
// https://wiki.unity3d.com/index.php/Toolbox

using System;
using System.Collections.Generic;
using UnityEngine;
 
public class GameController : Singleton<GameController>
{
    // Prevent instantiation - get instance with GameController.Instance
    protected GameController() {}

    // Tree of controllers
    private Dictionary<string, MonoBehaviour> m_controllers = 
        new Dictionary<string, MonoBehaviour>();  

    public enum StateType 
    {
        DEFAULT,    // Fall-back state, should never happen
        WAITING,    // Waiting for other player to finish selecting a character
        PLAYING,    // In-game
        PAUSED,     // In-game but one player has paused the game
        GAMEOVER, 
        GAMESTART,
        LOBBY,      // In lobby, connected to server
        MENU,       // Viewing in-game menu
        OPTIONS,    // Adjusting game options
    };

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