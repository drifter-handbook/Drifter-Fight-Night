﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkHost : MonoBehaviour
{
    // host ID is 0
    public int id { get; private set; } = 0;

    private class UDPClient
    {
        public int id = -1;
        public UDPHolePuncher.P2PClient client;
        public UDPConnection connection;
    }
    List<UDPClient> Clients { get; set; } = new List<UDPClient>();
    UDPConnection Receiver { get; set; }
    UDPHolePuncher HolePuncher { get; set; }

    public bool AcceptingClients { get; private set; } = true;

    Coroutine coroutine;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        HolePuncher = new UDPHolePuncher("68.187.67.135", "minecraft.scrollingnumbers.com", 6969, true);
        coroutine = StartCoroutine(Run());
    }
    IEnumerator Run()
    {
        while (AcceptingClients)
        {
            // accept clients
            List<UDPHolePuncher.P2PClient> clients = HolePuncher.ReceiveClients();
            foreach (UDPHolePuncher.P2PClient client in clients)
            {
                UDPConnection conn = new UDPConnection(client.SourceIP, client.SourcePort, client.DestIP, client.DestPort, true);
                if (Receiver == null)
                {
                    Receiver = new UDPConnection(client.SourceIP, client.SourcePort, client.DestIP, client.DestPort);
                }
                UDPClient newClient = new UDPClient { id = Clients.Count + 1, client = client, connection = conn };
                Clients.Add(newClient);
                Debug.Log($"Client {newClient.id} connected at {newClient.connection.udpSenderEp.ToString()}");
                AcceptingClients = false;
            }
            if (HolePuncher.Failed)
            {
                Debug.Log($"Failed to connect to server {HolePuncher.holePunchingServerName}:{HolePuncher.holePunchingServerPort}");
            }
            yield return null;
        }
        // send setup and ID to every client
        foreach (UDPClient client in Clients)
        {
            Debug.Log($"Sending pings to client at {client.connection.udpSenderEp.ToString()}");
            for (int i = 0; i < 5; i++)
            {
                client.connection.Send(Encoding.ASCII.GetBytes("Ping"));
                yield return new WaitForSeconds(0.1f);
            }
        }
        // pong clients
        List<UDPPacket> packets = Receiver.Receive();
        foreach (UDPPacket packet in packets)
        {
            if (Encoding.ASCII.GetString(packet.data) == "Ping")
            {
                Debug.Log($"Ping received from client {packet.address.ToString()}:{packet.port}");
                Clients[0].connection.Send(Encoding.ASCII.GetBytes("Pong"));
                break;
            }
        }
        yield return new WaitForSeconds(0.1f);
        // receive pongs on server
        packets = Receiver.Receive();
        foreach (UDPPacket packet in packets)
        {
            if (Encoding.ASCII.GetString(packet.data) == "Ping")
            {
                Debug.Log($"Pong received from client {packet.address.ToString()}:{packet.port}");
                break;
            }
        }
        yield return new WaitForSeconds(0.1f);
        // handle packets, by handing them out to each client
        /*
        while (true)
        {
            List<UDPPacket> packets = Receiver.Receive();
            foreach (UDPPacket packet in packets)
            {

            }
            yield return null;
        }
        */
    }
    // handle clients
    void HandleClient(UDPClient client)
    {

    }

    public void FinishAcceptingClients()
    {
        AcceptingClients = false;
        // kill hole puncher connection
        HolePuncher.Kill();
    }

    void OnDestroy()
    {
        // kill hole puncher connection
        HolePuncher?.Kill();
        // kill receiver
        Receiver?.Kill();
        // kill all client connections
        foreach (UDPClient client in Clients)
        {
            client?.connection.Kill();
        }
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}