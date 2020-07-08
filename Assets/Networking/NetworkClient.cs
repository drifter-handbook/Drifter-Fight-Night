using System;
using System.Collections;
using System.Collections.Generic;
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

    GameSyncManager sync;

    void Awake()
    {
        sync = GetComponent<GameSyncManager>();
    }

    void Start()
    {
        Network = new NetworkHandler(Host, HostID);
        // accept clients
        Network.OnConnect((addr, port, id) =>
        {
            Debug.Log($"Host visible at {addr}:{port}");
            GetComponent<MainPlayerSelect>().CharacterSelectState.Add(new CharacterSelectState());
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
        Network.OnReceive(new CharacterSelectInputPacket(), (id, packet) =>
        {
            if (PlayerID < 0)
            {
                return;
            }
            CharacterSelectState localCharSelect = new CharacterSelectState();
            if (PlayerID < GetComponent<MainPlayerSelect>().CharacterSelectState.Count)
            {
                localCharSelect = GetComponent<MainPlayerSelect>().CharacterSelectState[PlayerID];
            }
            GetComponent<MainPlayerSelect>().CharacterSelectState = ((CharacterSelectSyncPacket)packet).Data.CharacterSelectState;
            if (PlayerID < GetComponent<MainPlayerSelect>().CharacterSelectState.Count)
            {
                GetComponent<MainPlayerSelect>().CharacterSelectState[PlayerID] = localCharSelect;
            }
        }, true);
        // handle game input
        Network.OnReceive(new SyncToClientPacket(), (id, packet) =>
        {
            if (PlayerID < 0)
            {
                return;
            }
            GameSyncFromPacket((SyncToClientPacket)packet);
            // start game if not yet started
            if (!GameStarted)
            {
                StartGame();
            }
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
        if (sync.Entities == null)
        {
            CharacterSelectState localCharSelect = new CharacterSelectState();
            if (PlayerID < GetComponent<MainPlayerSelect>().CharacterSelectState.Count)
            {
                localCharSelect = GetComponent<MainPlayerSelect>().CharacterSelectState[PlayerID];
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
    void StartGame()
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
        sync.Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        // if we are client
        // remove all physics for synced objects
        foreach (GameObject obj in sync.Entities.players)
        {
            obj.GetComponent<Rigidbody2D>().simulated = false;
            obj.GetComponent<playerMovement>().IsClient = true;
        }
        foreach (GameObject obj in sync.Entities.objects)
        {
            obj.GetComponent<Rigidbody2D>().simulated = false;
        }
    }

    void GameSyncFromPacket(SyncToClientPacket data)
    {
        if (sync.Entities == null)
        {
            return;
        }
        foreach (GameObject player in sync.Entities.players)
        {
            if (player != null)
            {
                SyncToClientPacket.PlayerData playerData = data.SyncData.players.Find(x => x.name == player.name);
                if (playerData != null)
                {
                    player.GetComponent<PlayerSync>().SyncTo(playerData);
                    player.GetComponent<playerMovement>().SyncAnimatorState(playerData.animatorState);
                }
            }
        }
        foreach (GameObject obj in sync.Entities.objects)
        {
            if (obj != null)
            {
                SyncToClientPacket.ObjectData objData = data.SyncData.objects.Find(x => x.name == obj.name);
                if (objData != null)
                {
                    obj.GetComponent<ObjectSync>().SyncTo(objData);
                }
            }
        }
    }
}
