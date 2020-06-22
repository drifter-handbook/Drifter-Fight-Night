﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkHost : MonoBehaviour
{
    // host ID is 0
    public int id { get; private set; } = 0;

    private class HostedClient
    {
        public int ID = -1;
        public P2PClient client;
        public UDPConnection connection;
    }
    List<HostedClient> Clients { get; set; } = new List<HostedClient>();
    UDPConnection Receiver { get; set; }
    UDPHolePuncher HolePuncher { get; set; }

    public bool AcceptingClients { get; private set; } = true;

    Coroutine coroutine;

    public void Init()
    {
        coroutine = StartCoroutine(Run());
    }
    IEnumerator ConnectLAN()
    {
        IPAddress clientAddress = IPAddress.Parse("192.168.1.18");
        UdpClient udpClient = new UdpClient();
        byte[] data = GamePacketUtils.Serialize(new NoOpPacket());
        udpClient.Send(data, data.Length, new IPEndPoint(clientAddress, 7500));
        Receiver = new UDPConnection(udpClient, clientAddress, 7500);
        Clients.Add(new HostedClient() {
            ID = Clients.Count + 1,
            client = new P2PClient {
                UdpClient = udpClient,
                DestIP = clientAddress,
                DestPort = 7500
            },
            connection = Receiver
        });
        AcceptingClients = false;
        yield break;
    }
    IEnumerator ConnectHolePunch()
    {
        HolePuncher = new UDPHolePuncher("68.187.67.135", "minecraft.scrollingnumbers.com", 6969, true);
        while (AcceptingClients)
        {
            // accept clients
            List<P2PClient> clients = HolePuncher.ReceiveClients();
            foreach (P2PClient client in clients)
            {
                UDPConnection conn = new UDPConnection(client.UdpClient, client.DestIP, client.DestPort, true);
                if (Receiver == null)
                {
                    Receiver = new UDPConnection(client.UdpClient, client.DestIP, client.DestPort);
                }
                HostedClient newClient = new HostedClient { ID = Clients.Count + 1, client = client, connection = conn };
                Clients.Add(newClient);
                Debug.Log($"New client {newClient.ID} visible at {newClient.connection.udpSenderEp.ToString()}");
                AcceptingClients = false;
            }
            if (HolePuncher.Failed)
            {
                Debug.Log($"Failed to connect to server {HolePuncher.holePunchingServerName}:{HolePuncher.holePunchingServerPort}");
                yield break;
            }
            yield return null;
        }
    }
    IEnumerator Run()
    {
        yield return ConnectHolePunch();
        // receive requests to connect from clients
        bool finished = false;
        while (!finished)
        {
            List<UDPPacket> packets = Receiver.Receive();
            foreach (UDPPacket packet in packets)
            {
                IGamePacket gamePacket = GamePacketUtils.Deserialize(packet.data);
                if (gamePacket is ClientSetupPacket)
                {
                    // find matching client
                    HostedClient client = Clients.Find(x => x.client.DestIP.ToString() == packet.address.ToString() && x.client.DestPort == packet.port);
                    if (client != null)
                    {
                        Debug.Log($"Connection request received from client #{client.ID}");
                        client.connection.Send(GamePacketUtils.Serialize(new ClientSetupPacket() { ID = client.ID }));
                        finished = true;
                    }
                }
            }
            yield return null;
        }
        // handle client input packets
        float latest = 0;
        while (true)
        {
            List<UDPPacket> clientPackets = Receiver.Receive();
            foreach (UDPPacket packet in clientPackets)
            {
                IGamePacket gamePacket = GamePacketUtils.Deserialize(packet.data);
                if (gamePacket is InputToHostPacket)
                {
                    // only process most recent packets
                    if (latest < gamePacket.Timestamp)
                    {
                        latest = gamePacket.Timestamp;
                        GetComponent<GameSyncManager>().SetSyncInput((InputToHostPacket)gamePacket);
                    }
                }
            }
            yield return null;
        }
    }

    public void SendToClients(IGamePacket packet)
    {
        foreach (HostedClient client in Clients)
        {
            client.connection.Send(GamePacketUtils.Serialize(packet));
        }
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
        foreach (HostedClient client in Clients)
        {
            client?.connection.Kill();
        }
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}