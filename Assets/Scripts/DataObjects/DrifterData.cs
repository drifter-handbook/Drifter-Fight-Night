using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 
 * This will hold the data for each drifter. It is able to be modified
 * in the explorer.
 */
[CreateAssetMenu(fileName = "DrifterData", menuName = "VirtuaDrifter/DrifterData", order = 51)]
public class DrifterData : ScriptableObject
{
    [Header("UI Info")]
    [SerializeField] string readableName;
    float damageTaken {get; set;} = 0;
    
    [Header("Movement")]
    [SerializeField] int jumps;
    [SerializeField] float weight;
    [SerializeField] float runSpeed;
    [SerializeField] float walkSpeed;

    public string ReadableName { get { return readableName; }}
    public int Jumps { get { return jumps; }}
    public float Weight { get { return weight; }}
    public float RunSpeed { get { return runSpeed; }}
    public float WalkSpeed { get { return walkSpeed; }}
}