using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListButton : MonoBehaviour
{

	public Text roomCode;

    public void joinGameAsClient()
    {
    	if(roomCode.text != "")GameController.Instance.StartNetworkClient(roomCode.text);
    }
    
}
