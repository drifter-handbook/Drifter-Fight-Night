﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface NetworkID
{
    int PlayerID { get; }
    bool GameStarted { get; }
}

public class NetworkHost : MonoBehaviour, NetworkID
{
    public int PlayerID { get; private set; }
    public NetworkHandler Network { get; set; }

    NetworkEntityList entities = GameController.Instance.Entities;
    List<CharacterSelectState> CharacterSelectStates; // single source of truth

    void Start()
    {
        // create host network handler
        Network = new NetworkHandler();
        // accept clients
        Network.OnConnect((addr, port, id) =>
        {
            Debug.Log($"New client {id} visible at {addr}:{port}");
            // TODO: Add UI to networking when done
            // CharacterSelectStates.Add(new CharacterSelectState());
        });
        // on failure
        Network.OnFailure(() =>
        {
            Debug.Log($"Failed to connect to server" +
            $" {Network.HolePuncher.holePunchingServerName}:" +
            $" {Network.HolePuncher.holePunchingServerPort}");
        });
        // hand out client IDs
        // receive requests to connect from clients
        Network.OnReceive(new ClientSetupPacket(), (id, packet) =>
        {
            Debug.Log($"Connection request received from client #{id}");
            Network.Send(id, new ClientSetupPacket() { ID = id });
        });
        // handle character select
        Network.OnReceive(new CharacterSelectInputPacket(), (id, packet) =>
        {
            // TODO: Add UI to networking when done
            // CharacterSelectStates[id] = ((CharacterSelectInputPacket)packet).CharacterSelect;
        }, true);
        // handle game input
        Network.OnReceive(new InputToHostPacket(), (id, packet) =>
        {
            SetGameSyncInput((InputToHostPacket)packet, id);
        }, true);
        // start connection
        Network.Connect();
        StartCoroutine(TheCoolerFixedUpdate());
    }

    void Update()
    {
        Network.Update();
        if (GameStarted && Input.GetKeyDown(KeyCode.O))
        {
            GameStarted = false;
            StartGame();
        }
        Time.timeScale = GameController.Instance.IsPaused ? 0f : 1f;
    }

    void FixedUpdate()
    {
    }
    IEnumerator TheCoolerFixedUpdate()
    {
        float time = 0;
        float rate = 0.04f;
        while (true)
        {
            // run every 40ms
            yield return null;
            if (time > rate)
            {
                time -= rate;
            }
            else
            {
                continue;
            }
            // don't run if not connected
            if (PlayerID == -1)
            {
                continue;
            }
            if (entities == null)
            {
                // send character selection sync packet every frame
                Network.SendToAll(new CharacterSelectSyncPacket()
                {
                    Data = new CharacterSelectSyncPacket.CharacterSelectSyncData()
                    {
                        CharacterSelectState = CharacterSelectStates
                    }
                });
            }
            else
            {
                // send game sync packet every frame
                Network.SendToAll(CreateGameSyncPacket());
            }
        }
    }

    public bool GameStarted { get; private set; } = false;
    public void StartGame()
    {
        if (!GameStarted)
        {
            StartCoroutine(StartGameCoroutine());
        }
        GameStarted = true;
    }
    IEnumerator StartGameCoroutine()
    {
        yield return SceneManager.LoadSceneAsync("NetworkTestScene");
        // find entities
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        // create players
        List<string> playerNames = new List<string>() {
            "Spacejam",
            "Lady Parhelion",
            "Ryyke",
            "Spacejam",
           
        };

        //TODO: Grab player color from the list on the character Select screen (saved with player data)
        List<Color> playerColors = new List<Color>() {
            Color.red,
            Color.yellow,
            Color.green,
            Color.blue,
        };
        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject player = Instantiate(
                entities.GetEntityPrefab(playerNames[i]),
                entities.SpawnPoints[i].transform.position,
                entities.SpawnPoints[i].transform.rotation
            );
           Drifter drifter = player.GetComponent<Drifter>();
            drifter.SetColor(playerColors[i]);
            //player.GetComponentInChildren<SpriteRenderer>().color = playerColors[i];
            entities.AddPlayer(i, player);
        }
        // start game
        Network.StopAcceptingConnections();
        // attach player input to player 1
        GetComponent<PlayerInput>().input = entities.Players[0].GetComponent<Drifter>().input;
    }

    SyncToClientPacket CreateGameSyncPacket()
    {
        SyncToClientPacket.SyncToClientData SyncData = new SyncToClientPacket.SyncToClientData();
        for (int i = 0; i < entities.Entities.Count; i++)
        {
            GameObject entity = entities.Entities[i];
            if (entity != null)
            {
                INetworkSync entityData = entity.GetComponent<INetworkSync>();
                INetworkEntityData data = entityData.Serialize();
                data.Type = entityData.Type;
                SyncData.entities.Add(data);
            }
            // if game object is destroyed
            else
            {
                entities.Entities.RemoveAt(i);
                i--;
            }
        }
        SyncData.pause = GameController.Instance.IsPaused;
        return new SyncToClientPacket() { Timestamp = Time.time, SyncData = SyncData };
    }

    void SetGameSyncInput(InputToHostPacket input, int id)
    {
        if (entities != null && input?.input != null && entities.Players[id] != null)
        {
            entities.Players[id]?.GetComponent<Drifter>()?.input?.CopyFrom(input?.input);
        }
    }
}
