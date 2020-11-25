using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour    //TODO: Refactored, needs verification
{
    ScreenShake Shake;
    public Animator endgameBanner;
    //public List<int> deadByOrder = new List<int>(); //keeps track of who died in what order

    int startingPlayers;
    int currentPlayers;

    Dictionary<GameObject, int> playerList = new Dictionary<GameObject, int>();
    NetworkHost host;
    void Awake()
    {
        host = GameController.Instance.host;
        Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();
        //deadByOrder.Clear();

    }

    void Update()
    {
        if (NetworkPlayers.Instance != null && playerList.Count == 0)
        {
            foreach (KeyValuePair<int, GameObject> kvp in NetworkPlayers.Instance.players)
            {
                playerList.Add(kvp.Value, kvp.Key);
            }

            startingPlayers = playerList.Count;
            currentPlayers = startingPlayers;
        }
    }

    GameObject CreateExplosion(Collider2D other, int playerIndex)
    {
        GameObject deathExplosion = host.CreateNetworkObject("DeathExplosion", other.transform.position, Quaternion.identity);
        deathExplosion.transform.position =
            ClampObjectToScreenSpace.FindPosition(deathExplosion.transform);
        deathExplosion.transform.eulerAngles =
            ClampObjectToScreenSpace.FindNearestOctagonalAngle(deathExplosion.transform);
        return deathExplosion;    
    }

    void CreateHalo()
    {
        GameObject halo = host.CreateNetworkObject("HaloPlatform",
            new Vector2(0, 23),
            Quaternion.identity
        );
        halo.transform.localScale = new Vector2(10f, 10f);
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
            if (!drifter.status.HasStatusEffect(PlayerStatusEffect.DEAD))
            {
                StartCoroutine(Shake.Shake(.3f, 1.5f));

                drifter.Stocks--;
                drifter.DamageTaken = 0f;
                drifter.Charge = 0;
                drifter.status.ApplyStatusEffect(PlayerStatusEffect.DEAD, 1.9f);
                drifter.status.ApplyStatusEffect(PlayerStatusEffect.INVULN, 7f);

                if (other.gameObject.GetComponent<Drifter>().Stocks > 0)
                {
                    CreateExplosion(other, -1);
                    StartCoroutine(Respawn(other));
                }
                else
                {

                    currentPlayers--;

                    UnityEngine.Debug.Log("PLAYER " + other.gameObject.GetComponent<Drifter>().peerID + " KILLED! " + currentPlayers+ " OF " + startingPlayers + "PLAYERS REMAINING");

                    if(playerList.Count !=1)playerList.Remove(other.gameObject);

                    Destroy(other.gameObject);

                    if(playerList.Count ==1)
                    {

                        foreach (KeyValuePair<GameObject, int> kvp in playerList)
                        {
                            GameController.Instance.winner = kvp.Key.GetComponent<Drifter>().peerID;
                            endgameBanner.enabled = true;
                            
                        }

                        GameController.Instance.EndMatch(.8f);
                    }
                        
                }
            }
        }
    }
}