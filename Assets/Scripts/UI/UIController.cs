using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

 [DisallowMultipleComponent]
public class UIController : MonoBehaviour
{   
    // Character Select Source of truth - 1 obj for 1 player
    public List<CharacterSelectState> CharacterSelectState = 
        new List<CharacterSelectState>() { new CharacterSelectState() };
    
}
