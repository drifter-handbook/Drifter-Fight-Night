using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavvySyncManager : MonoBehaviour
{
    NetworkEntityList Entities;

    // Start is called before the first frame update
    void Start()
    {
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        GetComponent<PlayerInput>().input = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().input;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
