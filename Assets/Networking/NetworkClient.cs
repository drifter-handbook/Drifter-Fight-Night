using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkClient : MonoBehaviour, NetworkID
{
    public int PlayerID { get; private set; } = -1;
    public NetworkHandler Network { get; set; }
    NetworkTimer networkTimer = new NetworkTimer();
    NetworkPingTracker networkPing;

    const float ConnectTimeout = 3;
    Coroutine ConnectCoroutine;

    NetworkEntityList entities;
    List<CharacterSelectState> CharacterSelectStates => GameController.Instance.CharacterSelectStates; // single source of truth

    void Awake()
    {
        Network = new NetworkHandler(GameController.Instance.hostIP, GameController.Instance.HostID);

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
            // TODO: Add UI to networking when done
            if (PlayerID < 0)
            {
                return;
            }
            CharacterSelectState local = CharacterSelectStates.Find(x => x.PlayerID == PlayerID);
            GameController.Instance.CharacterSelectStates = ((CharacterSelectSyncPacket)packet).Data.CharacterSelectState;
            CharacterSelectState remote = CharacterSelectStates.Find(x => x.PlayerID == PlayerID);
            if (local != null && remote != null)
            {
                remote.PlayerType = local.PlayerType;
            }
            CharacterMenu menu = GameObject.FindGameObjectWithTag("CharacterMenu").GetComponent<CharacterMenu>();
            string s = ((CharacterSelectSyncPacket)packet).Data.stage;
            if (s != null && s != "" && !menu.GetComponent<Animator>().GetBool("location"))
            {
                menu.HeadToLocationSelect();
            }
            if (s != null && s != "")
            {
                menu.SelectFightzone(s);
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
                Debug.Log("Received start game packet");
                StartGame((SyncToClientPacket)packet);
            }
            GameSyncFromPacket((SyncToClientPacket)packet);
        }, true);
        // send input to host every 40ms
        networkTimer.Schedule(ProcessUpdate, 0.04f);
        networkTimer.Schedule(() => {
            // Debug.Log(syncsPerSecond);
            syncsPerSecond = 0;
        }, 1f);
        // ping and disconnect tracking
        networkPing = new NetworkPingTracker(Network, networkTimer);
        networkPing.OnDisconnect((id) =>
        {
            Debug.Log("Error: Connection to host lost.");
        });
        // start connection
        Network.Connect();
    }

    void Update()
    {
        Network.Update();
        networkTimer.Update(Time.realtimeSinceStartup);
    }

    void ProcessUpdate()
    {
        if (PlayerID == -1)
        {
            return;
        }
        GameObject pingUI = GameObject.FindGameObjectWithTag("PingDisplay");
        if (pingUI != null)
        {
            pingUI.GetComponentInChildren<Text>().text = ((int)Mathf.Round(networkPing.GetPing() * 1000f)).ToString();
        }
        // send char select input to host
        if (entities == null)
        {
            CharacterSelectState local = CharacterSelectStates.Find(x => x.PlayerID == PlayerID);
            Network.SendToAll(new CharacterSelectInputPacket()
            {
                CharacterSelect = local
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
            if (GameController.Instance.winner != null && GameController.Instance.winner != "")
            {
                EndGame();
            }
        }
    }

    IEnumerator ConnectToHost()
    {
        // Send request for a Client ID
        Debug.Log($"Sending connection request to host at {GameController.Instance.hostIP}");
        // Keep sending until we get a reply or timeout
        for (float time = 0; PlayerID == -1 && time < ConnectTimeout; time += Time.deltaTime)
        {
            Network.SendToAll(new ClientSetupPacket() { ID = -1 });
        }
        yield break;
    }

    public bool GameEnded { get; private set; } = false;
    public void EndGame()
    {
        if (!GameEnded)
        {
            StartCoroutine(EndGameCoroutine());
        }
        GameEnded = true;
    }
    IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(.7f);
        yield return SceneManager.LoadSceneAsync("Endgame");
        Text winner = GameObject.FindGameObjectWithTag("EndgameName").GetComponent<Text>();
        GameObject.FindGameObjectWithTag("EndgamePic").GetComponent<EndgameImageHandler>().setImage(GameController.Instance.winner);
        winner.text = $"Winner: {GameController.Instance.winner.Replace('_', ' ')}";
        while (SceneManager.GetActiveScene().name == "Endgame")
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                yield return SceneManager.LoadSceneAsync("MenuScene");
                GameController.Instance.selectedStage = null;
                GameController.Instance.winner = null;
                GameController.Instance.CharacterSelectStates = new List<CharacterSelectState>() { };
                GameController.Instance.Entities = null;
                Destroy(this);
                yield break;
            }
        }
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
        Debug.Log("Start scene load");
        yield return SceneManager.LoadSceneAsync(packet.SyncData.stage);
        Debug.Log("Finish scene load");
        // find entities
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        // create entities
        GameSyncFromPacket(packet);
        Debug.Log("Finish syncing");
        // remove all physics for synced objects
        foreach (GameObject obj in entities.Entities)
        {
            if (obj != null)
            {
                obj.GetComponent<Rigidbody2D>().simulated = false;
            }
        }
    }

    int syncsPerSecond = 0;
    void GameSyncFromPacket(SyncToClientPacket data)
    {
        syncsPerSecond++;
        if (entities == null)
        {
            return;
        }
        // if game over
        if (data.SyncData.winner != null && data.SyncData.winner != "")
        {
            GameController.Instance.winner = data.SyncData.winner;
            return;
        }
        Time.timeScale = data.SyncData.pause ? 0f : 1f;
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
                // disable client side simulation
                Rigidbody2D rb = entities.Entities[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = false;
                }
                // sync
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
