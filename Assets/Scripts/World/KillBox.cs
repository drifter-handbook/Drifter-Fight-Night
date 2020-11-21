using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour    //TODO: Refactored, needs verification
{
    ScreenShake Shake;
    public Animator endgameBanner;
    public List<int> deadByOrder = new List<int>(); //keeps track of who died in what order
    NetworkHost host;
    void Awake()
    {
        host = GameController.Instance.host;
        Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();
        deadByOrder.Clear();
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
            new Vector2(5, 23),
            Quaternion.identity
        );
        halo.transform.localScale = new Vector2(10f, 10f);
    }


    IEnumerator Respawn(Collider2D other)
    {
        
        yield return new WaitForSeconds(2f);
        CreateHalo();
        other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        other.transform.position = new Vector2(5f, 27f);
        yield break;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && GameController.Instance.IsHost && other.GetType() == typeof(BoxCollider2D))
        {
                Drifter drifter = other.gameObject?.GetComponent<Drifter>();

                StartCoroutine(Shake.Shake(.3f,1.5f));
            
                drifter.Stocks--;
                drifter.DamageTaken = 0f;
                drifter.Charge = 0;
                drifter.GetComponent<PlayerStatus>().ApplyStatusEffect(PlayerStatusEffect.DEAD,2f);
                drifter.GetComponent<PlayerStatus>().ApplyStatusEffect(PlayerStatusEffect.INVULN,3.5f);

                if (other.gameObject.GetComponent<Drifter>().Stocks > 0)
                {
                    CreateExplosion(other, -1);
                    StartCoroutine(Respawn(other));
                }
                else
                {
                    int destroyed = -1;
                    foreach (GameObject player in NetworkPlayers.Instance.players.Values)
                    {
                        if (player.Equals(other.gameObject))
                        {
                            destroyed = player.GetComponent<Drifter>().peerID;
                            deadByOrder.Add(player.GetComponent<Drifter>().peerID);
                            break;
                        }
                    }
                   
                    Destroy(other.gameObject);
                    // check for last one remaining
                    int count = 0;
                    int winner = -1;
                    foreach (GameObject go in NetworkPlayers.Instance.players.Values)
                    {
                        if (go.GetComponent<Drifter>().Stocks > 0)
                        {
                            winner = go.GetComponent<Drifter>().peerID;
                            count++;
                        }
                    }

                    //down to the last player! End game
                    if (count == 1)
                    {

                        endgameBanner.enabled = true;
                        GameController.Instance.winner = winner;
                        destroyed = -2;
               
                    }
                    else if (count < 1){
                    //There are no players with stocks left, default to the last killed
                    int victor = -1;
                        if (deadByOrder.Count > 0)
                        {
                            victor = deadByOrder[deadByOrder.Count - 1];
                            winner = deadByOrder[deadByOrder.Count - 1];
                        } else
                        {
                        //well...
                        UnityEngine.Debug.Log("I dunno fam, you really messed up.");
                        }

                        endgameBanner.enabled = true;
                        GameController.Instance.winner = winner;
                        destroyed = -2;
                }
                CreateExplosion(other, destroyed);
                    other.transform.position = new Vector2(0f,-300f);
                
            }
        }
    }
}
