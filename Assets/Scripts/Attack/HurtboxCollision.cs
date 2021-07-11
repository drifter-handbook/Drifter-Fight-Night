using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxCollision : MonoBehaviour
{
    public GameObject parent;
    public GameObject owner = null;
    // Start is called before the first frame update
    public CapsuleCollider2D capsule;
    void Start()
    {
        if(owner == null)owner = parent;
        capsule = GetComponentInChildren<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collider)
    {
    }
}
