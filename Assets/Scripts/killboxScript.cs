﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class killboxScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Destroy(other.gameObject);
            other.transform.position = new Vector3(1, 30, 1);
        }
    }
}
