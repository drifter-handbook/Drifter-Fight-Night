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

    
}
