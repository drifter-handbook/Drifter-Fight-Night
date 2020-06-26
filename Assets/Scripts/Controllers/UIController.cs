using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * This deals with all UI inputs
 */
 [DisallowMultipleComponent]
public class UIController : MonoBehaviour
{

    public enum Views {
        DEFAULT,            // Fall-back state, should never happen
        NONE,               // Don't show UI
        MENU,               // Viewing in-game menu
        OPTIONS,            // Adjusting game options
        CHARACTER_SELECT,   // Lobby
        PAUSED,             // Pause menu
        POST_MATCH,         // Show summary
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
