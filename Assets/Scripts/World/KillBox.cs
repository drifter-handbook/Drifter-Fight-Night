using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour    //TODO: Refactored, needs verification
{
    public ScreenShake Shake;
    //public List<int> deadByOrder = new List<int>(); //keeps track of who died in what order

    int startingPlayers;
    public int currentPlayers;

    //public int[] playerList = new int[0];

    //NetworkHost host;

    void Awake()
    {
        //host = GameController.Instance.host;
    }

    void Start(){
        startingPlayers = CombatManager.Instance.Drifters.Length;
        currentPlayers = startingPlayers;
    }

    GameObject CreateExplosion(Collider2D other, int playerIndex) {
        GameObject deathExplosion = GameController.Instance.CreatePrefab("DeathExplosion", other.transform.position, Quaternion.identity);
        deathExplosion.transform.position =
            ClampObjectToScreenSpace.FindPosition(deathExplosion.transform);
        deathExplosion.transform.eulerAngles = new Vector3(0,0,((other.gameObject.GetComponent<Rigidbody2D>().velocity.y>0)?1:-1) * Vector3.Angle(other.gameObject.GetComponent<Rigidbody2D>().velocity, new Vector3(1f,0,0)));
            //ClampObjectToScreenSpace.FindNearestOctagonalAngle(deathExplosion.transform);
        return deathExplosion;    
    }

    void OnTriggerExit2D(Collider2D other) {
        killPlayer(other);
    }


    protected void killPlayer(Collider2D other) {

    	if (other.gameObject.tag == "Player" && other.GetType() == typeof(BoxCollider2D))
        {
            while(Shake==null)Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();
            Drifter drifter = other.gameObject?.GetComponent<Drifter>();
            if (!drifter.status.HasStatusEffect(PlayerStatusEffect.DEAD)) {   

                Shake?.Shake(18, 1.5f);
                Shake?.Darken(6);
                CreateExplosion(other, -1);
                drifter.die();

                if (other.gameObject.GetComponent<Drifter>().Stocks <= 0) {
                    //UnityEngine.Debug.Log(drifter.peerID);
                    //playerList[drifter.peerID + 1] = currentPlayers;

                    currentPlayers--;

                    Destroy(other.gameObject);

                    if(currentPlayers <= 1)
                    {

                        //GameController.Instance.winnerOrder = playerList;

                        UnityEngine.Debug.Log("ENDING GAME");
                        
                        GameController.Instance.EndMatch();
                    }     
                }
            }
        }
    }
}