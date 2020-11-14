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
    void Awake()
    {
        Entities = GameObject.FindGameObjectWithTag(
            "NetworkEntityList").GetComponent<NetworkEntityList>();
        Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();
        deadByOrder.Clear();
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


    protected void killPlayer(Collider2D other){
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
                    int destroyed = -1;
                    foreach (CharacterSelectState state in GameController.Instance.CharacterSelectStates)
                    {
                    
                        if (Entities.Players.ContainsKey(state.PlayerID))
                        {
                        Entities.Players.TryGetValue(state.PlayerID, out GameObject obj);

                            if (obj.Equals(other.gameObject))
                            {
                                destroyed = state.PlayerIndex;
                                deadByOrder.Add(state);
                                break;
                            }
                        }
                    }
                   
                    Destroy(other.gameObject);
                // check for last one remaining
                    int count = 0;
                    int winner = -1;
                    foreach (GameObject go in Entities.Players.Values)
                    {
                        if (Entities.hasStocks(go))
                        {
                            int victor = -1;
                            foreach (CharacterSelectState select in GameController.Instance.CharacterSelectStates)
                            {
                                if (Entities.Players.ContainsKey(select.PlayerID) && go.Equals(Entities.Players[select.PlayerID]))
                                victor = select.PlayerID;
                            }
                            count++;
                        
                            winner = victor;
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
                            victor = deadByOrder[deadByOrder.Count - 1].PlayerIndex;
                            winner = deadByOrder[deadByOrder.Count - 1].PlayerID;
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
            	}
        	}
        }            
    } 
}
