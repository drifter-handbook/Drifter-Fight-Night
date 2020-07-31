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
    
    public bool Join() {
        if (GameController.Instance.SceneName != "CharacterSelect") return false;
        CharacterMenu menu = GameObject.Find("CharacterMenu").GetComponent<CharacterMenu>();
        if (Object.ReferenceEquals(menu, null)) return false;
        return menu.AddPlayerCard();
    }
    
}
