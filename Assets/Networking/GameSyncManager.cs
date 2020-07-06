using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
public class GameSyncManager : MonoBehaviour
{
    NetworkHost host;
    NetworkClient client;

    // set when game starts
    NetworkEntityList entities;

    // objects to sync
    [Header("Check box if hosting")]
    [SerializeField] private bool IsHost = false;

    public bool GameStarted { get; private set; } = false;
    public int ID => IsHost ? 0 : client.ID;

    public string HostIP = "68.187.67.135";
    public int HostID = 18;

    public bool GetIsHost()
    {
        return IsHost;
    }

    void Awake()
    {
        if (IsHost) { host = gameObject.AddComponent<NetworkHost>(); }
        else { client = gameObject.AddComponent<NetworkClient>(); }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        // if we are host
        if (IsHost)
        {
            host.Init();
        }
        else
        {
            client.Init(HostIP, HostID);
        }
    }

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
        // if we are host
        if (IsHost)
        {
            host.Init();
            // continue to simulate
            // attach player input to player 1
            GetComponent<PlayerInput>().input = entities.players[0].GetComponent<playerMovement>().input;
            foreach (GameObject obj in entities.players)
            {
                obj.GetComponent<playerMovement>().IsClient = false;
            }
        }
        // if we are client
        else
        {
            client.Init(HostIP, HostID);
            // remove all physics for synced objects
            foreach (GameObject obj in entities.players)
            {
                obj.GetComponent<Rigidbody2D>().simulated = false;
                obj.GetComponent<playerMovement>().IsClient = true;
            }
            foreach (GameObject obj in entities.objects)
            {
                obj.GetComponent<Rigidbody2D>().simulated = false;
            }
        }
    }

    void FixedUpdate()
    {
        // character select
        if (!GameStarted || entities == null)
        {
            // if host
            if (IsHost)
            {
                // send character selection sync packet every frame
                host.SendToClients(new CharacterSelectSyncPacket() {
                    Data = new CharacterSelectSyncPacket.CharacterSelectSyncData()
                    {
                        Players = GetComponent<MainPlayerSelect>().CharacterSelectState
                    }
                });
            }
            // if client
            else
            {
                // send client character selection every frame
                if (client.ID != -1)
                {
                    CharacterSelectState localCharSelect = new CharacterSelectState();
                    if (client.ID < GetComponent<MainPlayerSelect>().CharacterSelectState.Count)
                    {
                        localCharSelect = GetComponent<MainPlayerSelect>().CharacterSelectState[client.ID];
                    }
                    client.SendToHost(new CharacterSelectInputPacket()
                    {
                        CharacterSelect = localCharSelect
                    });
                }
            }
        }
        // game running
        else
        {
            // if host
            if (IsHost)
            {
                // send game sync packet every frame
                host.SendToClients(CreateGameSyncPacket());
            }
            // if client
            else
            {
                // send client game input every frame
                if (client.ID != -1)
                {
                    client.SendToHost(new InputToHostPacket()
                    {
                        input = (PlayerInputData)GetComponent<PlayerInput>().input.Clone()
                    });
                }
            }
        }
    }

    SyncToClientPacket CreateGameSyncPacket()
    {
        SyncToClientPacket.SyncToClientData SyncData = new SyncToClientPacket.SyncToClientData();
        foreach (GameObject player in entities.players)
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
        foreach (GameObject obj in entities.objects)
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

    public void GameSyncFromPacket(SyncToClientPacket data)
    {
        foreach (GameObject player in entities.players)
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
        foreach (GameObject obj in entities.objects)
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

    public void SetGameSyncInput(InputToHostPacket input, int id)
    {
        if (GameStarted && entities != null && input?.input != null)
        {
            entities.players[id]?.GetComponent<playerMovement>()?.input?.CopyFrom(input?.input);
        }
    }
}
