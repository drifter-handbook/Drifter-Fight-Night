using System.Collections;
using System.Collections.Generic;
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

    public bool AcceptingClients { get; private set; }

    Coroutine coroutine;

    public void Init()
    {
        HolePuncher = new UDPHolePuncher("127.0.0.1", "minecraft.scrollingnumbers.com", 6969, true);
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
                Clients.Add(new UDPClient { id = Clients.Count + 1, client = client, connection = conn });
            }
            yield return null;
        }
        // send setup and ID to every client
        foreach (UDPClient client in Clients)
        {
            // client.connection.Send();
        }
        // handle packets, by handing them out to each client
        while (true)
        {
            List<UDPPacket> packets = Receiver.Receive();
            foreach (UDPPacket packet in packets)
            {

            }
            yield return null;
        }
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
        HolePuncher.Kill();
        // kill receiver
        Receiver.Kill();
        // kill all client connections
        foreach (UDPClient client in Clients)
        {
            client.connection.Kill();
        }
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}
