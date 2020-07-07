using System.Collections;
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

    GameSyncManager sync;

    void Awake()
    {
        sync = GetComponent<GameSyncManager>();
    }

    void Start()
    {
        // create host network handler
        Network = new NetworkHandler();
        // accept clients
        Network.OnConnect((addr, port, id) =>
        {
            Debug.Log($"New client {id} visible at {addr}:{port}");
            GetComponent<MainPlayerSelect>().CharacterSelectState.Add(new CharacterSelectState());
        });
        // on failure
        Network.OnFailure(() =>
        {
            Debug.Log($"Failed to connect to server {Network.HolePuncher.holePunchingServerName}:{Network.HolePuncher.holePunchingServerPort}");
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
            GetComponent<MainPlayerSelect>().CharacterSelectState[id] = ((CharacterSelectInputPacket)packet).CharacterSelect;
        }, true);
        // handle game input
        Network.OnReceive(new InputToHostPacket(), (id, packet) =>
        {
            SetGameSyncInput((InputToHostPacket)packet, id);
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
        if (PlayerID != -1)
        {
            return;
        }
        if (sync.Entities == null)
        {
            // send character selection sync packet every frame
            Network.SendToAll(new CharacterSelectSyncPacket()
            {
                Data = new CharacterSelectSyncPacket.CharacterSelectSyncData()
                {
                    CharacterSelectState = GetComponent<MainPlayerSelect>().CharacterSelectState
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
        yield return SceneManager.LoadSceneAsync("NetworkTestScene");
        // find entities
        sync.Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        // start game
        Network.StopAcceptingConnections();
        // attach player input to player 1
        GetComponent<PlayerInput>().input = sync.Entities.players[0].GetComponent<playerMovement>().input;
        foreach (GameObject obj in sync.Entities.players)
        {
            obj.GetComponent<playerMovement>().IsClient = false;
        }
    }

    SyncToClientPacket CreateGameSyncPacket()
    {
        SyncToClientPacket.SyncToClientData SyncData = new SyncToClientPacket.SyncToClientData();
        foreach (GameObject player in sync.Entities.players)
        {
            if (player != null)
            {
                SyncData.players.Add(new SyncToClientPacket.PlayerData()
                {
                    name = player.gameObject.name,
                    x = player.transform.position.x,
                    y = player.transform.position.y,
                    z = player.transform.position.z,
                    facing = player.GetComponentInChildren<SpriteRenderer>().flipX,
                    animatorState = (PlayerAnimatorState)player.GetComponent<playerMovement>().animatorState.Clone()
                });
                player.GetComponent<playerMovement>().ResetAnimatorTriggers();
            }
        }
        foreach (GameObject obj in sync.Entities.objects)
        {
            if (obj != null)
            {
                SyncData.objects.Add(new SyncToClientPacket.ObjectData()
                {
                    name = obj.gameObject.name,
                    x = obj.transform.position.x,
                    y = obj.transform.position.y,
                    z = obj.transform.position.z,
                    angle = obj.transform.eulerAngles.z
                });
            }
        }
        return new SyncToClientPacket() { Timestamp = Time.time, SyncData = SyncData };
    }

    void SetGameSyncInput(InputToHostPacket input, int id)
    {
        if (sync.Entities != null && input?.input != null)
        {
            sync.Entities.players[id]?.GetComponent<playerMovement>()?.input?.CopyFrom(input?.input);
        }
    }
}
