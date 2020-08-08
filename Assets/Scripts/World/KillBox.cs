using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour    //TODO: Refactored, needs verification
{
    NetworkEntityList Entities;

    void Awake()
    {
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
    }

    void CreateExplosion(Collider2D other)
    {
        GameObject deathExplosion = Instantiate(
            GameController.Instance.deathExplosionPrefab,
            other.transform.position,
            Quaternion.identity
        );

        deathExplosion.transform.position =
            ClampObjectToScreenSpace.FindPosition(deathExplosion.transform);
        deathExplosion.transform.eulerAngles =
            ClampObjectToScreenSpace.FindNearestOctagonalAngle(deathExplosion.transform);

        Entities.AddEntity(deathExplosion.gameObject);
    }

    void Respawn()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Drifter drifter = other.gameObject?.GetComponent<Drifter>();

            CreateExplosion(other);
            // respawn

            drifter.Stocks--;
            drifter.DamageTaken = 0f;

            if (Entities.hasStocks(other.gameObject))
            {
                other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                other.transform.position = new Vector2(0, 25);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}
