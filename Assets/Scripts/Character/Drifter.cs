using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 
 * This is the class that will be put into a prefab and instantiated 
 */
 [RequireComponent(typeof(PlayerSync))]
 [RequireComponent(typeof(playerMovement))]
public class Drifter : MonoBehaviour
{
    public DrifterData drifterData;
    public playerMovement movement;
    public PlayerSync sync;
    public CustomControls controls;

    private void OnValidate() {
        if (Object.ReferenceEquals (sync, null)) {
             sync = gameObject.GetComponent<PlayerSync>();
        }

        if (Object.ReferenceEquals (movement, null)) {
            movement = gameObject.GetComponent<playerMovement>();
        }
    }

    private void Awake() {
    }

    private void Start() {
         
        if (Object.ReferenceEquals (drifterData, null)) {
            // Do something
            // Debug.LogError("No data found for " + this.gameObject.name);
        }
    }
}
