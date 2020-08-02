using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class killboxScript : MonoBehaviour
{
    public float deathAngle;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Destroy(other.gameObject);
            // create death explosion
            NetworkEntityList Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
            GameObject deathExplosion = Instantiate(Entities.GetEntityPrefab("DeathExplosion"),
                other.transform.position,
                Quaternion.identity);
            deathExplosion.transform.eulerAngles = new Vector3(0f, 0f, deathAngle);
            Entities.AddEntity(deathExplosion);
        }
    }
}
