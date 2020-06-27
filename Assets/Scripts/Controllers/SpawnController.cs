using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dynamically creates spawn points for
[ExecuteInEditMode] 
[DisallowMultipleComponent]
public class SpawnController : MonoBehaviour
{
    Queue<Transform> spawnpoints;
    public int example = 0; 
    [SerializeField][Range(0, 13)] int numSpawns = 2; // Range 13 due to camera size

    private void Awake() {
        numSpawns = 0;
        spawnpoints = new Queue<Transform>();
    }

    void Update()
    {
        // Plz no null ref thx u_u
        if(Object.ReferenceEquals (spawnpoints, null)) {
            spawnpoints = new Queue<Transform>();
        }
        // TODO: Fix reload bug
        while(spawnpoints.Count != numSpawns) {
            if(numSpawns > spawnpoints.Count) {
                CreateSpawnPoint();
            } else {
                DestroySpawnPoints();
            }
        }
    }

   void CreateSpawnPoint() {
        // Instantiates game object
        Transform newSpawn = new GameObject().transform;
        // Keep track of it using a queue (allows us to manage order easier)
        spawnpoints.Enqueue(newSpawn);
        
        // Initial placement 
        float calculatedPos = Mathf.Ceil(spawnpoints.Count / 2);
        calculatedPos = spawnpoints.Count % 2 != 0 ? calculatedPos * -1 : calculatedPos;
        newSpawn.transform.SetPositionAndRotation(new Vector3(calculatedPos, 0, 0), newSpawn.transform.rotation);

        newSpawn.name = "SpawnPoint";    
        
        newSpawn.transform.parent = this.gameObject.transform;
    }

    void DestroySpawnPoints(){
        if (spawnpoints.Count < 1) return;

        // you are most certainly not allowed to change this variable name
        Transform dannyDeleto = spawnpoints.Dequeue();
        DestroyImmediate(dannyDeleto.gameObject);
    }

    // TODO: Probably define the type of this parameter to not the most generic type
    void SpawnCharacters(List<Transform> players) {
        // TODO
    }
}
