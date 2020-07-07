using System.Collections;
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

    public bool AcceptingClients = true;

    Coroutine coroutine;

    public int PlayerID;

    public void Init()
    {
        coroutine = StartCoroutine(Run());
    }
    IEnumerator ConnectHolePunch()
    {
        HolePuncher = new UDPHolePuncher("68.187.67.135", "minecraft.scrollingnumbers.com", 6969, true, 0);
        PlayerID = HolePuncher.ID;
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
                GetComponent<MainPlayerSelect>().CharacterSelectState.Add(new CharacterSelectState());
                Debug.Log($"New client {newClient.ID} visible at {newClient.connection.udpSenderEp.ToString()}");
            }
            // hand out client IDs
            // receive requests to connect from clients
            if (Receiver != null)
            {
                List<UDPPacket> packets = Receiver.Receive();
                foreach (UDPPacket packet in packets)
                {
                    IGamePacket gamePacket = GamePacketUtils.Deserialize(packet.data);
                    HostedClient client = Clients.Find(x => x.client.DestIP.ToString() == packet.address.ToString() && x.client.DestPort == packet.port);
                    if (gamePacket is ClientSetupPacket)
                    {
                        // find matching client
                        if (client != null)
                        {
                            Debug.Log($"Connection request received from client #{client.ID}");
                            client.connection.Send(GamePacketUtils.Serialize(new ClientSetupPacket() { ID = client.ID }));
                        }
                    }
                    // handle character select
                    Dictionary<string, Dictionary<int, float>> latest = new Dictionary<string, Dictionary<int, float>>();
                    if (!latest.ContainsKey(gamePacket.TypeID))
                    {
                        latest[gamePacket.TypeID] = new Dictionary<int, float>();
                    }
                    if (!latest[gamePacket.TypeID].ContainsKey(client.ID))
                    {
                        latest[gamePacket.TypeID][client.ID] = 0f;
                    }
                    if (gamePacket is CharacterSelectInputPacket)
                    {
                        // only process most recent packets
                        if (latest[gamePacket.TypeID][client.ID] < gamePacket.Timestamp)
                        {
                            latest[gamePacket.TypeID][client.ID] = gamePacket.Timestamp;
                            GetComponent<MainPlayerSelect>().CharacterSelectState[client.ID] = ((CharacterSelectInputPacket)gamePacket).CharacterSelect;
                        }
                    }
                }
            }
            // check for hole punch failure
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
        // handle client input packets
        Dictionary<string, Dictionary<int, float>> latest = new Dictionary<string, Dictionary<int, float>>();
        while (true)
        {
            List<UDPPacket> clientPackets = Receiver.Receive();
            foreach (UDPPacket packet in clientPackets)
            {
                IGamePacket gamePacket = GamePacketUtils.Deserialize(packet.data);
                HostedClient client = Clients.Find(x => x.client.DestIP.ToString() == packet.address.ToString() && x.client.DestPort == packet.port);
                if (!latest.ContainsKey(gamePacket.TypeID))
                {
                    latest[gamePacket.TypeID] = new Dictionary<int, float>();
                }
                if (!latest[gamePacket.TypeID].ContainsKey(client.ID))
                {
                    latest[gamePacket.TypeID][client.ID] = 0f;
                }
                if (gamePacket is InputToHostPacket)
                {
                    // only process most recent packets
                    if (latest[gamePacket.TypeID][client.ID] < gamePacket.Timestamp)
                    {
                        latest[gamePacket.TypeID][client.ID] = gamePacket.Timestamp;
                        GetComponent<GameSyncManager>().SetGameSyncInput((InputToHostPacket)gamePacket, client.ID);
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
