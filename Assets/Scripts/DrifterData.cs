using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DrifterData", menuName = "VirtuaDrifter/DrifterData", order = 51)]
public class DrifterData : ScriptableObject
{
    [SerializeField] string readableName;
    [SerializeField] Sprite sprite;
    
    [Header("Movement")]
    [SerializeField] int jumps;
    [SerializeField] float weight;
    [SerializeField] float speed; // Feel free to break this into states
    
    [Header("Basic Attack")] 
    [SerializeField] float knockback;
    [SerializeField] float damage; 
    [SerializeField] float damageTaken;
}