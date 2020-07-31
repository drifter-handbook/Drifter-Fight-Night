using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxCollision : MonoBehaviour
{
    public GameObject parent;
    // Start is called before the first frame update
    CapsuleCollider2D capsule;
    void Start()
    {
      capsule = GetComponentInChildren<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collider)
    {
       Debug.Log("hi");
     }
}
