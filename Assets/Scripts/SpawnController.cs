using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dynamically creates spawn points for 
[ExecuteInEditMode] public class SpawnController : MonoBehaviour
{
    Queue<Transform> spawnpoints;
    [SerializeField][Range(0, 13)] int numSpawns = 2; // Range 13 due to camera size

    private void Awake() {
        spawnpoints = new Queue<Transform>();
    }

    void Update()
    {
        // Plz no null ref thx u_u
        if(Object.ReferenceEquals (spawnpoints, null)) {
            spawnpoints = new Queue<Transform>();
        }
        Debug.Log(spawnpoints);
        // TODO: Is this off by 1?
        while(spawnpoints.Count != numSpawns) {
            if(numSpawns > spawnpoints.Count) {
                CreateSpawnPoint();
            } else {
                DestroySpawnPoints();
            }
        }
    }

   void CreateSpawnPoint() {
        // If anyone wants to clean this up please do
        Transform newSpawn = new GameObject().transform;
        spawnpoints.Enqueue(newSpawn);
        newSpawn.name = "SpawnPoint";
        bool negative = spawnpoints.Count % 2 != 0;
        float calculatedPos = Mathf.Ceil(spawnpoints.Count / 2);
        calculatedPos = negative ? calculatedPos * -1 : calculatedPos;
        newSpawn.transform.SetPositionAndRotation(new Vector3(calculatedPos, 0, 0), newSpawn.transform.rotation);
        newSpawn.transform.parent = this.gameObject.transform;
    }

    void DestroySpawnPoints(){
        // you are most certainly not allowed to change this variable name
        Transform dannyDeleto = spawnpoints.Dequeue();
        DestroyImmediate(dannyDeleto.gameObject);
    }

    // TODO: Probably define the type of this parameter to not the most generic type
    void SpawnCharacters(List<Transform> players) {
        // TODO
    }
}
