using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    // -1 id for unassigned
    public int id { get; private set; } = -1;

    const float TIMEOUT = 3;

    public UDPConnection Host { get; private set; }
    public UDPHolePuncher HolePuncher { get; private set; }

    Coroutine coroutine;

    public void Init(string host)
    {
        coroutine = StartCoroutine(Run(host));
    }
    IEnumerator ConnectLAN(string host)
    {
        UdpClient udpClient = new UdpClient();
        byte[] data = GamePacketUtils.Serialize(new NoOpPacket());
        udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(host), 7500));
        Host = new UDPConnection(udpClient, IPAddress.Parse(host), 7500);
        yield break;
    }
    IEnumerator ConnectHolePunch(string host)
    {
        HolePuncher = new UDPHolePuncher(host, "minecraft.scrollingnumbers.com", 6969, false);
        // connect to host
        while (Host == null)
        {
            List<P2PClient> hosts = HolePuncher.ReceiveClients();
            if (hosts.Count > 0)
            {
                Host = new UDPConnection(hosts[0].UdpClient, hosts[0].DestIP, hosts[0].DestPort);
                Debug.Log($"Attempting to connect to host at {Host.udpSenderEp.ToString()}");
            }
            if (HolePuncher.Failed)
            {
                Debug.Log($"Failed to connect to server {HolePuncher.holePunchingServerName}:{HolePuncher.holePunchingServerPort}");
                yield break;
            }
            yield return null;
        }
    }
    IEnumerator Run(string host)
    {
        yield return ConnectHolePunch(host);
        // talk to host and receive a client ID
        yield return Setup();
        if (id == -1)
        {
            // failed to communicate with host
            throw new InvalidOperationException($"Failed to connect to host at {Host.udpSenderEp.ToString()}");
        }
        // receive host sync packets
        float latest = 0;
        while (true)
        {
            List<UDPPacket> hostPackets = Host.Receive();
            foreach (UDPPacket packet in hostPackets)
            {
                IGamePacket gamePacket = GamePacketUtils.Deserialize(packet.data);
                if (gamePacket is SyncToClientPacket)
                {
                    // only process most recent packets
                    if (latest < gamePacket.Timestamp)
                    {
                        latest = gamePacket.Timestamp;
                        GetComponent<GameSyncManager>().SyncFromPacket((SyncToClientPacket)gamePacket);
                    }
                }
            }
            yield return null;
        }
    }

    IEnumerator Setup()
    {
        // Send request for a Client ID
        Debug.Log($"Sending connection request to host at {Host.udpSenderEp.ToString()}");
        // Receive Client ID from Host
        for (float time = 0; id == -1 && time < TIMEOUT; time += Time.deltaTime)
        {
            SendToHost(new ClientSetupPacket() { ID = -1 });
            yield return null;
            List<UDPPacket> packets = Host.Receive();
            foreach (UDPPacket packet in packets)
            {
                IGamePacket gamePacket = GamePacketUtils.Deserialize(packet.data);
                if (gamePacket is ClientSetupPacket)
                {
                    id = ((ClientSetupPacket)gamePacket).ID;
                    Debug.Log($"Connected to host at {packet.address.ToString()}:{packet.port}, we are Client #{id}");
                    SendToHost(gamePacket);
                    break;
                }
            }
        }
        yield break;
    }

    public void SendToHost(IGamePacket packet)
    {
        Host.Send(GamePacketUtils.Serialize(packet));
    }

    void OnApplicationQuit()
    {
        // kill hole puncher connection
        HolePuncher?.Kill();
        // kill connection with host
        Host?.Kill();
    }
}
