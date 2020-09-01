using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour    //TODO: Refactored, needs verification
{
    NetworkEntityList Entities;
    public Animator endgameBanner;

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
                //int destroyed = 0;
                foreach (CharacterSelectState state in GameController.Instance.CharacterSelectStates)
                {
                    
                    if (Entities.Players.ContainsKey(state.PlayerID))
                    {
                        GameObject obj;
                        Entities.Players.TryGetValue(state.PlayerID, out obj);

                        if (obj.Equals(other.gameObject))
                        {
                            
                           // destroyed = state.PlayerIndex;
                        }
                    }
                }
                
                Destroy(other.gameObject);
                // check for last one remaining
                int count = 0;
                foreach (GameObject go in Entities.Players.Values)
                {
                    if (Entities.hasStocks(go))
                    {
                        int victor = -1;
                        foreach (CharacterSelectState select in GameController.Instance.CharacterSelectStates)
                        {
                            if (Entities.Players.ContainsKey(select.PlayerID) && go.Equals(Entities.Players[select.PlayerID]))
                                victor = select.PlayerIndex;
                        }
                        count++;
                        GameController.Instance.winner = go.GetComponent<INetworkSync>().Type + "|" + victor;
                    }
                }
                if (count != 1)
                {
                    //gameObject.GetComponentInParent<MultiSound>().PlayAudio(destroyed);
                    GameController.Instance.winner = null;
                } else
                {
                    //gameObject.GetComponentInParent<SingleSound>().PlayAudio();
                    endgameBanner.enabled = true;
                }
            }
        }
    }
}
