using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * This deals with all UI inputs
 */
 [DisallowMultipleComponent]
public class UIController : MonoBehaviour
{

    // Character Select Source of truth
    public List<CharacterSelectState> CharacterSelectState = new List<CharacterSelectState>() { new CharacterSelectState() };


    public enum Views {
        DEFAULT,            // Fall-back state, should never happen
        NONE,               // Don't show UI
        MENU,               // Viewing in-game menu
        OPTIONS,            // Adjusting game options
        CHARACTER_SELECT,   // Lobby
        PAUSED,             // Pause menu
        POST_MATCH,         // Show summary
    }

    
}
