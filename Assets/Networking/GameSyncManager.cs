﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSyncManager : MonoBehaviour
{
    NetworkHost host;
    NetworkClient client;

    // objects to sync
    public List<GameObject> networkPlayers;
    public List<GameObject> networkObjects;

    public bool IsHost { get; private set; } = false;

    void Awake()
    {
        host = GetComponent<NetworkHost>();
        client = GetComponent<NetworkClient>();
        if (host != null)
        {
            IsHost = true;
        }
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
        }
        // if we are client
        else
        {
            client.Init("68.187.67.135");
            // remove all physics for synced objects
            foreach (GameObject obj in networkPlayers)
            {
                obj.GetComponent<Rigidbody2D>().simulated = false;
            }
            // attach player input to player 2
            GetComponent<PlayerInput>().input = networkPlayers[1].GetComponent<playerMovement>().input;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
                    input = networkPlayers[1].GetComponent<playerMovement>().input
                });
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
                facing = player.GetComponent<SpriteRenderer>().flipX
            });
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
