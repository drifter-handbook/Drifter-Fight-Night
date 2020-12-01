using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MatchmakingHost : MonoBehaviour
{
    NetworkHost host => GameController.Instance.host;

    Dictionary<string, bool> sentNatPunch = new Dictionary<string, bool>();

    [NonSerialized]
    public bool roomIsPublic = true;

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
        // create room
        UnityWebRequest www = UnityWebRequest.Post($"{server}/rooms/{GameController.Instance.Username}/{(roomIsPublic ? 1 : 0)}", "");
        yield return www.SendWebRequest();
        MatchmakingCreateResponse createResponse = null;
        if (www.isNetworkError || www.isHttpError)
        {
            throw new UnityException(www.error);
        }
        else
        {
            createResponse = JsonConvert.DeserializeObject<MatchmakingCreateResponse>(www.downloadHandler.text);
            host.ConnectionKey = createResponse.connection_key;
            host.RoomKey = createResponse.room_code;
            Debug.Log($"Room code: {createResponse.room_code}");
        }
        // continuously refresh room until start
        while (!host.GameStarted)
        {
            www = UnityWebRequest.Post($"{server}/refresh/{createResponse.user_id}", "");
            yield return www.SendWebRequest();
            MatchmakingRefreshResponse refreshResponse = null;
            if (www.isNetworkError || www.isHttpError)
            {
                throw new UnityException(www.error);
            }
            else
            {
                refreshResponse = JsonConvert.DeserializeObject<MatchmakingRefreshResponse>(www.downloadHandler.text);
                if (refreshResponse != null)
                {
                    // connect to new clients
                    foreach (string natPunchCode in refreshResponse.connect)
                    {
                        if (!sentNatPunch.ContainsKey(natPunchCode))
                        {
                            sentNatPunch[natPunchCode] = true;
                            host.netManager.NatPunchModule.SendNatIntroduceRequest(GameController.Instance.NatPunchServer, natPunchCode);
                        }
                    }
                }
            }
            // host refreshes a lot since it has to look for new clients
            yield return new WaitForSeconds(1.0f);
        }
        // game start and the like is handled by NetworkHost
        // keep room open for next play
        while (host.GameStarted)
        {
            www = UnityWebRequest.Post($"{server}/refresh/{createResponse.user_id}", "");
            yield return www.SendWebRequest();
            MatchmakingRefreshResponse refreshResponse = null;
            if (www.isNetworkError || www.isHttpError)
            {
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

public class MatchmakingCreateResponse
{
    public string room_code;
    public string user_id;
    public string connection_key;
}

public class MatchmakingRefreshResponse
{
    public List<string> connect;
    public bool expired;
}