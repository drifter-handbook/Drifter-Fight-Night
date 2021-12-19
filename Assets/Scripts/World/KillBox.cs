using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour    //TODO: Refactored, needs verification
{
    public ScreenShake Shake;
    public Animator endgameBanner;
    //public List<int> deadByOrder = new List<int>(); //keeps track of who died in what order

    int startingPlayers;
    public int currentPlayers;

    public int[] playerList = new int[0];

    NetworkHost host;

    void Awake()
    {
        host = GameController.Instance.host;
    }

    void Update()
    {
        if (GameController.Instance.IsHost && NetworkPlayers.Instance != null && playerList.Length == 0)
        {
            GameController.Instance.winnerOrder = new int[0];

            playerList = new int[NetworkPlayers.Instance.players.Count]; 
            foreach (KeyValuePair<int, GameObject> kvp in NetworkPlayers.Instance.players)
                if(kvp.Key <=4) playerList[kvp.Key +1] = 1;
    

            startingPlayers = NetworkPlayers.Instance.players.Count;
            currentPlayers = startingPlayers;
        }
    }

    GameObject CreateExplosion(Collider2D other, int playerIndex)
    {
        GameObject deathExplosion = host.CreateNetworkObject("DeathExplosion", other.transform.position, Quaternion.identity);
        deathExplosion.transform.position =
            ClampObjectToScreenSpace.FindPosition(deathExplosion.transform);
        deathExplosion.transform.eulerAngles = new Vector3(0,0,((other.gameObject.GetComponent<Rigidbody2D>().velocity.y>0)?1:-1) * Vector3.Angle(other.gameObject.GetComponent<Rigidbody2D>().velocity, new Vector3(1f,0,0)));
            //ClampObjectToScreenSpace.FindNearestOctagonalAngle(deathExplosion.transform);
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
        yield return new WaitForSeconds(.083f);
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
            while(Shake==null)Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();
            Drifter drifter = other.gameObject?.GetComponent<Drifter>();
            if (!drifter.status.HasStatusEffect(PlayerStatusEffect.DEAD))
            {   

                Shake?.startShakeCoroutine(.3f, 1.5f);
                Shake?.startDarkenCoroutine(.1f);

                drifter.Stocks--;
                drifter.DamageTaken = 0f;
                drifter.superCharge = 2f;
                drifter.status.ApplyStatusEffect(PlayerStatusEffect.DEAD, 1.9f);
                drifter.status.ApplyStatusEffect(PlayerStatusEffect.INVULN, 7f);

                CreateExplosion(other, -1);
                           

                if (other.gameObject.GetComponent<Drifter>().Stocks > 0)
                {
                    StartCoroutine(Respawn(other));
                }
                else
                {
                    //UnityEngine.Debug.Log(drifter.peerID);
                    playerList[drifter.peerID + 1] = currentPlayers;

                    currentPlayers--;

                    Destroy(other.gameObject);

                    if(currentPlayers <= 1)
                    {

                        GameController.Instance.winnerOrder = playerList;

                        endgameBanner.enabled = true;

                        UnityEngine.Debug.Log("ENDING GAME");
                        
                        GameController.Instance.EndMatch(.8f);
                    }     
                }
            }
        }
    }
}