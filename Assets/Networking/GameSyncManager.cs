using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSyncManager : MonoBehaviour
{
    NetworkHost host;
    NetworkClient client;

    // objects to sync
    public List<GameObject> networkPlayers;
    public List<GameObject> networkObjects;
    [Header("Check box if hosting")]
    [SerializeField] private bool IsHost = false;

    public bool GetIsHost(){
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
        // if we are host
        if (IsHost)
        {
            host.Init();
            // continue to simulate
            // attach player input to player 1
            GetComponent<PlayerInput>().input = networkPlayers[0].GetComponent<playerMovement>().input;
            foreach (GameObject obj in networkPlayers)
            {
                obj.GetComponent<playerMovement>().IsClient = false;
            }
        }
        // if we are client
        else
        {
            client.Init("75.134.27.221");
            // remove all physics for synced objects
            foreach (GameObject obj in networkPlayers)
            {
                obj.GetComponent<Rigidbody2D>().simulated = false;
                obj.GetComponent<playerMovement>().IsClient = true;
            }
            foreach (GameObject obj in networkObjects)
            {
                obj.GetComponent<Rigidbody2D>().simulated = false;
            }
            // attach player input to player 2
            GetComponent<PlayerInput>().input = networkPlayers[1].GetComponent<playerMovement>().input;
        }
    }

    void FixedUpdate()
    {
        // if host
        if (IsHost)
        {
            // send sync packet every frame
            host.SendToClients(CreateSyncPacket());
        }
        // if client
        else
        {
            // send client input every frame
            if (client.id != -1)
            {
                client.SendToHost(new InputToHostPacket()
                {
                    input = (PlayerInputData)networkPlayers[1].GetComponent<playerMovement>().input.Clone()
                });
                GetComponent<PlayerInput>().input.ResetKeyDowns();
            }
        }
    }

    SyncToClientPacket CreateSyncPacket()
    {
        SyncToClientPacket.SyncToClientData SyncData = new SyncToClientPacket.SyncToClientData();
        foreach (GameObject player in networkPlayers)
        {
            SyncData.players.Add(new SyncToClientPacket.PlayerData() {
                name = player.gameObject.name,
                x = player.transform.position.x,
                y = player.transform.position.y,
                z = player.transform.position.z,
                facing = player.GetComponentInChildren<SpriteRenderer>().flipX,
                animatorState = (PlayerAnimatorState)player.GetComponent<playerMovement>().animatorState.Clone()
            });
            player.GetComponent<playerMovement>().ResetAnimatorTriggers();
        }
        foreach (GameObject obj in networkObjects)
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
        return new SyncToClientPacket() { Timestamp = Time.time, SyncData = SyncData };
    }

    public void SyncFromPacket(SyncToClientPacket data)
    {
        foreach (GameObject player in networkPlayers)
        {
            SyncToClientPacket.PlayerData playerData = data.SyncData.players.Find(x => x.name == player.name);
            if (playerData != null)
            {
                player.GetComponent<PlayerSync>().SyncTo(playerData);
                player.GetComponent<playerMovement>().SyncAnimatorState(playerData.animatorState);
            }
        }
        foreach (GameObject obj in networkObjects)
        {
            SyncToClientPacket.ObjectData objData = data.SyncData.objects.Find(x => x.name == obj.name);
            if (objData != null)
            {
                obj.GetComponent<ObjectSync>().SyncTo(objData);
            }
        }
    }

    public void SetSyncInput(InputToHostPacket input)
    {
        networkPlayers[1].GetComponent<playerMovement>().input = input.input;
    }
}
