using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 
 * This is the class that will be put into a prefab and instantiated 
 */
public class Drifter : MonoBehaviour
{

    public DrifterData drifterData;
    public playerMovement movement;
    public PlayerSync sync;



    private void Awake() {
        if (Object.ReferenceEquals (sync, null)) {
             sync = gameObject.AddComponent<PlayerSync>();
        }

        if (Object.ReferenceEquals (movement, null)) {
            movement = gameObject.AddComponent<playerMovement>();
        }
        if (Object.ReferenceEquals (drifterData, null)) {
            Debug.LogError("No data found for " + this.gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
