﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : MonoBehaviour, NetworkID
{
    public string Host;
    public int HostID;

    public int PlayerID { get; private set; } = -1;
    public NetworkHandler Network { get; set; }

    const float ConnectTimeout = 3;
    Coroutine ConnectCoroutine;

    NetworkEntityList entities = GameController.Instance.Entities;
    List<CharacterSelectState> CharacterSelectStates; // single source of truth

    void Start()
    {
        Network = new NetworkHandler(Host, HostID);
        CharacterSelectStates = GetComponent<GameController>().CharacterSelectStates;

        // accept clients
        Network.OnConnect((addr, port, id) =>
        {
            Debug.Log($"Host visible at {addr}:{port}");
            CharacterSelectStates.Add(new CharacterSelectState());
            // attempt to connect to host and obtain a PlayerID
            if (ConnectCoroutine == null)
            {
                ConnectCoroutine = StartCoroutine(ConnectToHost());
            }
        });

        // on failure
        Network.OnFailure(() =>
        {
            Debug.Log($"Failed to connect to server {Network.HolePuncher.holePunchingServerName}:{Network.HolePuncher.holePunchingServerPort}");
        });

        // handle client setup
        Network.OnReceive(new ClientSetupPacket(), (id, packet) =>
        {
            PlayerID = ((ClientSetupPacket)packet).ID;
            Debug.Log($"Connected to host at {packet.address.ToString()}:{packet.port}, we are Client #{PlayerID}");
            // attach player input to player with ID
            GetComponent<PlayerInput>().input = new PlayerInputData();
        });

        // handle character select
        Network.OnReceive(new CharacterSelectSyncPacket(), (id, packet) =>
        {
            if (PlayerID < 0)
            {
                return;
            }
            CharacterSelectState localCharSelect = new CharacterSelectState();
            if (PlayerID < CharacterSelectStates.Count)
            {
                localCharSelect = CharacterSelectStates[PlayerID];
            }
            CharacterSelectStates = ((CharacterSelectSyncPacket)packet).Data.CharacterSelectState;
            if (PlayerID < CharacterSelectStates.Count)
            {
                CharacterSelectStates[PlayerID] = localCharSelect;
            }
        }, true);
        // handle game input
        Network.OnReceive(new SyncToClientPacket(), (id, packet) =>
        {
            if (PlayerID < 0)
            {
                return;
            }
            // start game if not yet started
            if (!GameStarted)
            {
                StartGame((SyncToClientPacket)packet);
            }
            GameSyncFromPacket((SyncToClientPacket)packet);
        }, true);
        // start connection
        Network.Connect();
    }

    void Update()
    {
        Network.Update();
    }

    void FixedUpdate()
    {
        if (PlayerID == -1)
        {
            return;
        }
        // send char select input to host
        if (entities == null)
        {
            CharacterSelectState localCharSelect = new CharacterSelectState();
            if (PlayerID < CharacterSelectStates.Count)
            {
                localCharSelect = CharacterSelectStates[PlayerID];
            }
            Network.SendToAll(new CharacterSelectInputPacket()
            {
                CharacterSelect = localCharSelect
            });
        }
        // send game input to host
        else
        {
            // send client game input every frame
            Network.SendToAll(new InputToHostPacket()
            {
                input = (PlayerInputData)GetComponent<PlayerInput>().input.Clone()
            });
        }
    }

    IEnumerator ConnectToHost()
    {
        // Send request for a Client ID
        Debug.Log($"Sending connection request to host at {Host}");
        // Keep sending until we get a reply or timeout
        for (float time = 0; PlayerID == -1 && time < ConnectTimeout; time += Time.deltaTime)
        {
            Network.SendToAll(new ClientSetupPacket() { ID = -1 });
        }
        yield break;
    }

    public bool GameStarted { get; set; } = false;
    void StartGame(SyncToClientPacket packet)
    {
        if (!GameStarted)
        {
            StartCoroutine(StartGameCoroutine(packet));
        }
        GameStarted = true;
    }
    IEnumerator StartGameCoroutine(SyncToClientPacket packet)
    {
        yield return SceneManager.LoadSceneAsync("NetworkTestScene");
        // find entities
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        // create entities
        GameSyncFromPacket(packet);
        // remove all physics for synced objects
        foreach (GameObject obj in entities.Entities)
        {
            obj.GetComponent<Rigidbody2D>().simulated = false;
        }
    }

    void GameSyncFromPacket(SyncToClientPacket data)
    {
        if (entities == null)
        {
            return;
        }
        // if in packet data but not in current entities, create
        foreach (INetworkEntityData entityData in data.SyncData.entities)
        {
            if (!entities.Entities.Any(x => x != null && x.GetComponent<INetworkSync>().ID == entityData.ID))
            {
                GameObject entity = Instantiate(entities.GetEntityPrefab(entityData.Type));
                entity.GetComponent<INetworkSync>().ID = entityData.ID;
                entities.Entities.Add(entity);
            }
        }
        // sync objects
        for (int i = 0; i < entities.Entities.Count; i++)
        {
            if (entities.Entities[i] != null)
            {
                INetworkSync entitySync = entities.Entities[i].GetComponent<INetworkSync>();
                if (entitySync != null)
                {
                    INetworkEntityData entityData = data.SyncData.entities.Find(x => x.ID == entitySync.ID);
                    // if in packet data and in current entities, sync
                    if (entityData != null)
                    {
                        entitySync.Deserialize(entityData);
                    }
                    // if not in packet data but in current entities, destroy
                    else
                    {
                        Destroy(((MonoBehaviour)entitySync).gameObject);
                        entities.Entities.RemoveAt(i);
                        i--;
                    }
                }
            }
            // if game object null for some reason
            else
            {
                entities.Entities.RemoveAt(i);
                i--;
            }
        }
    }
}
