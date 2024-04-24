using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using HeathenEngineering.SteamworksIntegration;

public class MainMenuManager : MonoBehaviour {
	[SerializeField] private GameObject joinPrefab;
	[SerializeField] private Transform joinParent;
	[SerializeField] private GameObject noLobbies;

	// Start is called before the first frame update
	void Start() {
		GetFriendLobbies();
	}

	public void RefreshLobbies() {
		for (int i = 0; i < joinParent.childCount; i++)
			Destroy(joinParent.GetChild(i).gameObject);


		GetFriendLobbies();
	}

	void GetFriendLobbies() {
		int lobbiesAdded = 0;
		Debug.Log($"friends {SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate)}");
		for (int i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate); i++) {
			CSteamID friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
			if (SteamFriends.GetFriendGamePlayed(friendId, out var gameInfo)
			&& gameInfo.m_steamIDLobby.IsValid()
			&& gameInfo.m_gameID.AppID() == SteamSettings.ApplicationId) {
				Debug.Log("attempting to add lobby");
				AddJoinLobby(gameInfo.m_steamIDLobby, $"{SteamFriends.GetFriendPersonaName(friendId)}'s Lobby");
				lobbiesAdded++;
			}

		}

		if (lobbiesAdded == 0)
			noLobbies.SetActive(true);
		else
			noLobbies.SetActive(false);
	}


	private void AddJoinLobby(CSteamID lobbyID, string nameOverride = null) {
		string name = SteamMatchmaking.GetLobbyData(lobbyID, "name");
		if (nameOverride != null)
			name = nameOverride; 

		GameObject button = Instantiate(joinPrefab, joinParent);
		
		button.GetComponentInChildren<TextMeshProUGUI>().text = name;
		button.GetComponent<Button>().onClick.AddListener(JoinLobby);

		void JoinLobby() {
			SteamMatchmaking.JoinLobby(lobbyID);
		}
	}

	public void OnQuit() {
		Application.Quit();
	}

}
