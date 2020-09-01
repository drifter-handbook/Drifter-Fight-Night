﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour    //TODO: Refactored, needs verification
{
    NetworkEntityList Entities;

    void Awake()
    {
        Entities = GameObject.FindGameObjectWithTag(
            "NetworkEntityList").GetComponent<NetworkEntityList>();
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

    void CreateHalo()
    {
        GameObject halo = Instantiate(
            Entities.GetEntityPrefab("HaloPlatform"),
            new Vector2(5, 23),
            Quaternion.identity
        );
        halo.transform.localScale = new Vector2(10f,10f);
        Entities.AddEntity(halo.gameObject);
    }


    IEnumerator Respawn(Collider2D other)
    {
        yield return new WaitForSeconds(2f);
        CreateHalo();
        other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        other.transform.position = new Vector2(5, 27);
        yield break;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && GameController.Instance.IsHost)
        {
            Drifter drifter = other.gameObject?.GetComponent<Drifter>();

            CreateExplosion(other);

            drifter.Stocks--;
            drifter.DamageTaken = 0f;
            drifter.Charge = 0;
            drifter.GetComponent<PlayerStatus>().ApplyStatusEffect(PlayerStatusEffect.DEAD,2f);
            drifter.GetComponent<PlayerStatus>().ApplyStatusEffect(PlayerStatusEffect.INVULN,3.5f);

            if (Entities.hasStocks(other.gameObject))
            {
                 StartCoroutine(Respawn(other));
            }
            else
            {
                Destroy(other.gameObject);
                // check for last one remaining
                int count = 0;
                foreach (GameObject go in Entities.Players.Values)
                {
                    if (Entities.hasStocks(go))
                    {
                        count++;
                        GameController.Instance.winner = go.GetComponent<INetworkSync>().Type;
                    }
                }
                if (count != 1)
                {
                    GameController.Instance.winner = null;
                }
            }
        }
    }
}
