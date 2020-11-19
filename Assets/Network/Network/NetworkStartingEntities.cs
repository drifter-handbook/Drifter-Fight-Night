using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkStartingEntities : MonoBehaviour
{
    public List<GameObject> startingEntities;

    void Awake()
    {
        foreach (GameObject obj in startingEntities)
        {
            if (obj.GetComponent<GameController>() == null)
            {
                obj.SetActive(false);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
