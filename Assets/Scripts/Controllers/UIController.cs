using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

 [DisallowMultipleComponent]
public class UIController : MonoBehaviour
{
    
    // Character Select Source of truth - 1 obj for 1 player
    private List<CharacterSelectState> m_CSSList = 
        new List<CharacterSelectState>() { new CharacterSelectState() };
    
    public List<CharacterSelectState> CharacterSelectState
    {
        get {return m_CSSList;}
        set {
            if (m_CSSList == value) return;
            m_CSSList = value;
            if (OnPlayersChanged != null)
                OnPlayersChanged();
        }
    }
    public delegate void OnVariableChangeDelegate();
    public event OnVariableChangeDelegate OnPlayersChanged;

     private void Start()
    {
        this.OnPlayersChanged += PlayerChangeHandler;
    }
    
    private void PlayerChangeHandler()
    {
        CharacterMenu cm = GameObject.Find("CharacterMenu").GetComponent<CharacterMenu>();
        if(Object.ReferenceEquals(cm, null)) {
            Debug.LogWarning("Character Menu not found!");
            return;
        }
        
        cm.AddPlayerCard();
    }
}
