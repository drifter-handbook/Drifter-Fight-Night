using System.Linq;
using System.Collections;
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
    NetworkTimer networkTimer = new NetworkTimer();
    NetworkPingTracker networkPing;

    NetworkEntityList entities = GameController.Instance.Entities;

    // single source of truth, indexed by PlayerIndex
    List<CharacterSelectState> CharacterSelectStates => GameController.Instance.CharacterSelectStates;

    void Awake()
    {
        PlayerID = 0;
        CharacterSelectStates.Add(new CharacterSelectState() {
            PlayerID = 0,
            PlayerIndex = 0,
        });
        // create host network handler
        Network = new NetworkHandler();
        // accept clients
        Network.OnConnect((addr, port, id) =>
        {
            // get next player index
            int nextPlayerIndex = -1;
            for (int i = 0; i < GameController.MAX_PLAYERS; i++)
            {
                bool taken = false;
                foreach (CharacterSelectState player in CharacterSelectStates)
                {
                    if (player.PlayerIndex == i)
                    {
                        taken = true;
                        break;
                    }
                }
                if (!taken)
                {
                    nextPlayerIndex = i;
                    break;
                }
            }
            Debug.Log($"New client {id} visible at {addr}:{port}");
            // add client to UI
            CharacterSelectStates.Add(new CharacterSelectState()
            {
                PlayerID = id,
                PlayerIndex = nextPlayerIndex
            });
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
            CharacterSelectState state = CharacterSelectStates.Find(x => x.PlayerID == id);
            CharacterSelectState clientCharSel = ((CharacterSelectInputPacket)packet).CharacterSelect;
            if (state != null && state.PlayerIndex >= 0 && clientCharSel != null)
            {
                CharacterSelectStates[state.PlayerIndex].PlayerType = clientCharSel.PlayerType;
            }
            // TODO: what about join and drop?
        }, true);
        // handle game input
        Network.OnReceive(new InputToHostPacket(), (id, packet) =>
        {
            CharacterSelectState state = CharacterSelectStates.Find(x => x.PlayerID == id);
            SetGameSyncInput((InputToHostPacket)packet, state.PlayerIndex);
        }, true);
        // send updates every 40ms
        networkTimer.Schedule(ProcessUpdate, 0.04f);
        // ping and disconnect tracking
        networkPing = new NetworkPingTracker(Network, networkTimer);
        networkPing.OnDisconnect((id) =>
        {
            Debug.Log($"Error: Connection to client {id} lost.");
            // remove from char select state
            CharacterSelectStates.RemoveAll(x => x.PlayerID == id);
        });
        // start connection
        Network.Connect();
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
        // update
        networkTimer.Update(Time.realtimeSinceStartup);
    }

    void FixedUpdate()
    {
    }

    void ProcessUpdate()
    {
        // don't run if not connected
        if (PlayerID == -1)
        {
            return;
        }
        if (entities == null)
        {
            // sort
            for (int i = 0; i < CharacterSelectStates.Count; i++)
            {
                CharacterSelectStates[i].PlayerIndex = i;
            }
            // send character selection sync packet every frame
            Network.SendToAll(new CharacterSelectSyncPacket()
            {
                Data = new CharacterSelectSyncPacket.CharacterSelectSyncData()
                {
                    CharacterSelectState = CharacterSelectStates,
                    stage = GameController.Instance.selectedStage
                }
            });
        }
        else
        {
            // send game sync packet every frame
            Network.SendToAll(CreateGameSyncPacket());
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
        Debug.Log("Start scene load");
        yield return SceneManager.LoadSceneAsync(GameController.Instance.selectedStage);
        Debug.Log("Finish scene load");
        // find entities
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        // create players
        List<string> playerNames = CharacterSelectStates.Select(x => x.PlayerType.ToString().Replace("_", " ")).ToList();
        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject player = Instantiate(
                entities.GetEntityPrefab(playerNames[i]),
                entities.SpawnPoints[i % entities.SpawnPoints.Count].transform.position,
                Quaternion.identity
            );
            Drifter drifter = player.GetComponent<Drifter>();
            drifter.SetColor(CharacterMenu.ColorFromEnum[(PlayerColor)i]);
            // player.GetComponentInChildren<SpriteRenderer>().color = playerColors[i];
            entities.AddPlayer(i, player);
        }
        Debug.Log("Added players");
        // start game
        Network.StopAcceptingConnections();
        // attach player input to player 1
        GetComponent<PlayerInput>().input = entities.Players[0].GetComponent<Drifter>().input;
        Debug.Log("Attached input");
    }

    SyncToClientPacket CreateGameSyncPacket()
    {
        //Debug.Log("Sync");
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
        SyncData.stage = SceneManager.GetActiveScene().name;
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
