using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListButton : MonoBehaviour
{

	public Text roomName;
	string roomCode;
	public float keepAlive = 0;

	void Update()
	{
		keepAlive += Time.deltaTime;
		if(keepAlive >= 10f)Destroy(gameObject);
	}

	public void init(string name,string code)
	{
		roomName.text = name;
		roomCode = code;
	}

    public void joinGameAsClient()
    {
    	if(roomCode != "")GameController.Instance.StartNetworkClient(roomCode);
    }
    
}
