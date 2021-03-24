using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MatchmakingClient : MonoBehaviour
{
    [NonSerialized]
    public string JoinRoom = null;

    NetworkClient client => GameController.Instance.client;

    void Start()
    {
        StartCoroutine(PollMatchmakingServer());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator PollMatchmakingServer()
    {
        string server = $"http://{GameController.Instance.MatchmakingServer.Address.ToString()}:{GameController.Instance.MatchmakingServer.Port}";
        // join room
        UnityWebRequest www = UnityWebRequest.Post($"{server}/join/{JoinRoom}/{GameController.Instance.Username}", "");
        yield return www.SendWebRequest();
        MatchmakingJoinResponse joinResponse = null;
        if (www.isNetworkError || www.isHttpError)
        {
            // TODO: room has closed error or something, or host left,
            // or bad room code
            UnityEngine.Debug.Log("ROOM CLEANED");

            GameController.Instance.CleanupNetwork();
            throw new UnityException("Invalid room code.");



        }
        else
        {
            joinResponse = JsonConvert.DeserializeObject<MatchmakingJoinResponse>(www.downloadHandler.text);
            if (joinResponse != null)
            {
                // connect to new clients
                client.ConnectionKey = joinResponse.connection_key;
                client.netManager.NatPunchModule.SendNatIntroduceRequest(GameController.Instance.NatPunchServer, joinResponse.connect);
            }
        }
        // keep room open for next play
        while (client.GameStarted)
        {
            yield return www.SendWebRequest();
            MatchmakingRefreshResponse refreshResponse = null;
            if (www.isNetworkError || www.isHttpError)
            {
                // TODO: room closed
                throw new UnityException(www.error);
            }
            else
            {
                refreshResponse = JsonConvert.DeserializeObject<MatchmakingRefreshResponse>(www.downloadHandler.text);
                if (refreshResponse.expired)
                {
                    throw new UnityException("Lost spot in matchmaking server.");
                }
            }
            // refresh only enough to keep our spot
            yield return new WaitForSeconds(8f);
        }
    }
}

public class MatchmakingJoinResponse
{
    public string user_id;
    public string connection_key;
    public string connect;
}