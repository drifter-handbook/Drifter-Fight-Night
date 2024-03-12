using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;

public class MatchmakingUI : MonoBehaviour
{
    //string roomCode = "";
    public GameObject roomListHolder;
    public GameObject roomHolder;

    Dictionary<string,RoomListButton> roomList = new Dictionary<string,RoomListButton>();

    //Coroutine getRoomsCoroutine;

    // Start is called before the first frame update
    // void Start()
    // {
    //     getRoomsCoroutine = StartCoroutine(GetRoomsCoroutine());
    // }


    // IEnumerator GetRoomsCoroutine()
    // {
    //     string server = $"http://{GameController.Instance.MatchmakingServer.Address.ToString()}:{GameController.Instance.MatchmakingServer.Port}";
    //     // get rooms
    //     while (true)
    //     {
    //         UnityWebRequest www = UnityWebRequest.Get($"{server}/rooms/{GameController.Instance.Username}/1");
    //         yield return www.SendWebRequest();
    //         List<MatchmakingRoomEntry> roomEntries = null;
    //         if (www.isNetworkError || www.isHttpError)
    //         {
    //             throw new UnityException(www.error);
    //         }
    //         else
    //         {
    //             roomEntries = JsonConvert.DeserializeObject<List<MatchmakingRoomEntry>>(www.downloadHandler.text);
    //             if (roomEntries != null)
    //             {
    //                 foreach (MatchmakingRoomEntry room in roomEntries)
    //                 {
    //                     if(room.users <=0)
    //                     {
    //                         Destroy(roomList[room.room_code].gameObject);
    //                     }
    //                     else
    //                     {
    //                         Debug.Log($"[Room Entry] {room.name}: {room.room_code}, {room.users}/8");
    //                         if(!roomList.ContainsKey(room.room_code))
    //                         {
    //                             GameObject newRoom = Instantiate(roomHolder, new Vector3(0,0), Quaternion.identity);

    //                             newRoom.GetComponent<RoomListButton>().init(room.name,room.room_code);
    //                             newRoom.transform.SetParent(roomListHolder.transform, false); 
    //                             roomList[room.room_code] = newRoom.GetComponent<RoomListButton>();
    //                         }
    //                         else
    //                         {
    //                             roomList[room.room_code].keepAlive = 0;
    //                         }
    //                     }

    //                     roomList = roomList
    //                     .Where(f => f.Value != null)
    //                     .ToDictionary(x => x.Key, x => x.Value);

    //                     //Populate room codes here
    //                 }
    //             }
    //         }
    //         // refresh every so often for new rooms
    //         yield return new WaitForSeconds(5f);
    //     }
    // }

    // public void SetRoomCode()
    // {
    //     roomCode = clientRoomCode.text;
    // }

    //0 = online
    //1 = Local
    //2 = Training
    //3 = story?

    public void StartHost(int mode = 0)
    {
        //GameController.Instance.IsHost = true;
        //GameController.Instance.IsOnline = (mode ==0);
        GameController.Instance.IsTraining = (mode == 2);
        //GameController.Instance.StartNetworkHost();
        //StopCoroutine(getRoomsCoroutine);

        GameController.Instance.GoToCharacterSelect();
        GameController.Instance.StartGGPO();
    }

    public void refresh()
    {
        //StopCoroutine(getRoomsCoroutine);
        List<RoomListButton> keyList = roomList.Values.ToList();
        foreach(RoomListButton button in keyList)
        {
            Destroy(button.gameObject);
        }
        roomList = new Dictionary<string,RoomListButton>();

        //getRoomsCoroutine = StartCoroutine(GetRoomsCoroutine());

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
