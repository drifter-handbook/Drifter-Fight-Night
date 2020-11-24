using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour    //TODO: Refactored, needs verification
{
    NetworkEntityList Entities;
    ScreenShake Shake;
    public Animator endgameBanner;
    public List<CharacterSelectState> deadByOrder = new List<CharacterSelectState>(); //keeps track of who died in what order

    Dictionary<GameObject,int> playerList = new Dictionary<GameObject,int>();
    
    void Awake()
    {
        Entities = GameObject.FindGameObjectWithTag(
            "NetworkEntityList").GetComponent<NetworkEntityList>();
        Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();
        deadByOrder.Clear();

    }

    void Start()
    {
        foreach (KeyValuePair<int, GameObject> kvp in Entities.Players)
        {
            playerList.Add(kvp.Value,kvp.Key);
        }
    }

    GameObject CreateExplosion(Collider2D other, int playerIndex)
    {
        GameController.Instance.deathExplosionPrefab.GetComponent<DeathExplosionSync>().PlayerIndex = playerIndex;
        GameObject deathExplosion = Instantiate(
            GameController.Instance.deathExplosionPrefab,
            other.transform.position,
            Quaternion.identity
        );
        GameController.Instance.deathExplosionPrefab.GetComponent<DeathExplosionSync>().PlayerIndex = -1;
        deathExplosion.transform.position =
            ClampObjectToScreenSpace.FindPosition(deathExplosion.transform);
        deathExplosion.transform.eulerAngles =
            ClampObjectToScreenSpace.FindNearestOctagonalAngle(deathExplosion.transform);

        Entities.AddEntity(deathExplosion.gameObject);
        return deathExplosion;    
    }

    void CreateHalo()
    {
        GameObject halo = Instantiate(
            Entities.GetEntityPrefab("HaloPlatform"),
            new Vector2(0, 23),
            Quaternion.identity
        );
        halo.transform.localScale = new Vector2(10f,10f);
        Entities.AddEntity(halo.gameObject);
    }


    IEnumerator Respawn(Collider2D other)
    {
        other.transform.position = new Vector2(0f, 150f);
        yield return new WaitForSeconds(2f);
        CreateHalo();
        other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        other.transform.position = new Vector2(0f, 27f);
        yield break;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        killPlayer(other);
    }


    protected void killPlayer(Collider2D other)
    {

    	if (other.gameObject.tag == "Player" && GameController.Instance.IsHost && other.GetType() == typeof(BoxCollider2D))
        {
            Drifter drifter = other.gameObject?.GetComponent<Drifter>();
            if(!drifter.status.HasStatusEffect(PlayerStatusEffect.DEAD))
            {
                StartCoroutine(Shake.Shake(.3f,1.5f));
            
                drifter.Stocks--;
                drifter.DamageTaken = 0f;
                drifter.Charge = 0;
                drifter.status.ApplyStatusEffect(PlayerStatusEffect.DEAD,1.9f);
                drifter.status.ApplyStatusEffect(PlayerStatusEffect.INVULN,7f);

                if (Entities.hasStocks(other.gameObject))
                {
                    CreateExplosion(other, -1);
                    StartCoroutine(Respawn(other));
                }
                else
                {   

                    UnityEngine.Debug.Log(playerList.Count);

                    CreateExplosion(other, -1); 

                    if(playerList.Count != 1)
                    {
                        playerList.Remove(other.gameObject);

                        Destroy(other.gameObject);
                    }


                    if(playerList.Count == 1)
                    {
                        foreach (KeyValuePair<int, GameObject> kvp in Entities.Players)
                        {
                            GameController.Instance.winner = kvp.Key;
                            endgameBanner.enabled = true;
                        }

                    }

                }

            }
        }
    } 
}
