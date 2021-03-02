using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MatchmakingUI : MonoBehaviour
{
    //string roomCode = "";
    public GameObject roomListHolder;
    public GameObject roomHolder;

    List<string> roomList = new List<string>();

    Coroutine getRoomsCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        getRoomsCoroutine = StartCoroutine(GetRoomsCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GetRoomsCoroutine()
    {
        string server = $"http://{GameController.Instance.MatchmakingServer.Address.ToString()}:{GameController.Instance.MatchmakingServer.Port}";
        // get rooms
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get($"{server}/rooms/{GameController.Instance.Username}/1");
            yield return www.SendWebRequest();
            List<MatchmakingRoomEntry> roomEntries = null;
            if (www.isNetworkError || www.isHttpError)
            {
                throw new UnityException(www.error);
            }
            else
            {
                roomEntries = JsonConvert.DeserializeObject<List<MatchmakingRoomEntry>>(www.downloadHandler.text);
                if (roomEntries != null)
                {
                    foreach (MatchmakingRoomEntry room in roomEntries)
                    {
                        Debug.Log($"[Room Entry] {room.name}: {room.room_code}, {room.users}/8");

                        if(!roomList.Contains(room.room_code))
                        {
                            GameObject newRoom = Instantiate(roomHolder, new Vector3(0,0), Quaternion.identity);

                            newRoom.GetComponent<Text>().text = room.room_code;
                            newRoom.transform.SetParent(roomListHolder.transform, false); 

                            roomList.Add(room.room_code);
                        }
                        else
                        {
                            //Update existing entry
                        }

                    
                        //Populate room codes here
                    }
                }
            }
            // refresh every so often for new rooms
            yield return new WaitForSeconds(5f);
        }
    }

    // public void SetRoomCode()
    // {
    //     roomCode = clientRoomCode.text;
    // }

    public void StartHost()
    {
        GameController.Instance.IsHost = true;
        GameController.Instance.StartNetworkHost();
        StopCoroutine(getRoomsCoroutine);
        GameController.Instance.host.SetScene("CharacterSelect");
    }

    // public void StartClient()
    // {
    //     SetRoomCode();
    //     GameController.Instance.StartNetworkClient(roomCode);
    // }
}

public class MatchmakingRoomEntry
{
    public string name;
    public string room_code;
    public int users;
}
