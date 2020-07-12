using System;
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
            GetComponent<UIController>().CharacterSelectState.Add(new CharacterSelectState());
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
            if (PlayerID < GetComponent<UIController>().CharacterSelectState.Count)
            {
                localCharSelect = GetComponent<UIController>().CharacterSelectState[PlayerID];
            }
            GetComponent<UIController>().CharacterSelectState = ((CharacterSelectSyncPacket)packet).Data.CharacterSelectState;
            if (PlayerID < GetComponent<UIController>().CharacterSelectState.Count)
            {
                GetComponent<UIController>().CharacterSelectState[PlayerID] = localCharSelect;
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
        if (sync.Entities == null)
        {
            CharacterSelectState localCharSelect = new CharacterSelectState();
            if (PlayerID < GetComponent<UIController>().CharacterSelectState.Count)
            {
                localCharSelect = GetComponent<UIController>().CharacterSelectState[PlayerID];
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
        sync.Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        // create entities
        GameSyncFromPacket(packet);
        // remove all physics for synced objects
        foreach (GameObject obj in sync.Entities.Players.Values)
        {
            obj.GetComponent<playerMovement>().IsClient = true;
        }
        foreach (GameObject obj in sync.Entities.Entities)
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
        // if in packet data but not in current entities, create
        foreach (INetworkEntityData entityData in data.SyncData.entities)
        {
            if (!sync.Entities.Entities.Any(x => x != null && x.GetComponent<INetworkSync>().ID == entityData.ID))
            {
                GameObject entity = Instantiate(sync.Entities.GetEntityPrefab(entityData.Type));
                entity.GetComponent<INetworkSync>().ID = entityData.ID;
                sync.Entities.Entities.Add(entity);
            }
        }
        // sync objects
        for (int i = 0; i < sync.Entities.Entities.Count; i++)
        {
            INetworkSync entitySync = sync.Entities.Entities[i]?.GetComponent<INetworkSync>();
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
                    sync.Entities.Entities.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
