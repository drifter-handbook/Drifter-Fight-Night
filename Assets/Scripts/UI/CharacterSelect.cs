using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour {
    public CharacterMenu menu;

    public static KeyValuePair<PlayerData, bool> LocalPlayer; // playerData, isHost

    List<PlayerData> players = new List<PlayerData>();

    public bool PlayerJoin(int connectionID) {
        PlayerData dannyDe_add_o = new PlayerData(connectionID);
        players.Add(dannyDe_add_o);
        return menu.AddPlayerCard(dannyDe_add_o);
    }

    public void PlayerDisconnect(int connectionID) {
        PlayerData dannyDeleto = players.Find(player => player.PlayerID == connectionID);
        players.Remove(dannyDeleto);
        menu.RemovePlayerCard(dannyDeleto.PlayerIndex);
    }

    // TODO: Remove when done

    [Range(0,8)] private static int id = 0;
    [SerializeField] Text text;

    public void Tester_PlayerJoin() {
        text.text = id.ToString();
        PlayerJoin(id);
        if(id == 0) {
            LocalPlayer = new KeyValuePair<PlayerData, bool>(players[0], true);
            print("created Local player");
        }
        id += 1;        
    }

    public void Tester_PlayerDisconnect () { // NetworkID.playerID
        PlayerDisconnect(id);
        id -= 1;
        text.text = id.ToString();
    }
}