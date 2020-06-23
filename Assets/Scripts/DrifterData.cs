using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DrifterData", menuName = "VirtuaDrifter/DrifterData", order = 51)]
public class DrifterData : ScriptableObject
{
    [Header("UI Info")]
    [SerializeField] string readableName;
    [SerializeField] Sprite playerSprite;
    float damageTaken {get; set;} = 0;
    
    [Header("Movement")]
    [SerializeField] int jumps;
    [SerializeField] float weight;
    [SerializeField] float speed; // Feel free to break this into states
    
    [Header("Basic Attack")] 
    [SerializeField] float knockback;
    [SerializeField] float damage; 

    public string ReadableName { get { return readableName; }}
    public Sprite PlayerSprite { get { return playerSprite; }}
    public int Jumps { get { return jumps; }}
    public float Weight { get { return weight; }}
    public float Speed { get { return speed; }}
    public float Knockback { get { return knockback; }}
    public float Damage { get { return damage; }}
}