using System;
using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        Init("68.187.67.135");
    }

    public void Init(string host)
    {
        HolePuncher = new UDPHolePuncher(host, "minecraft.scrollingnumbers.com", 6969, false);
        coroutine = StartCoroutine(Run());
    }
    IEnumerator Run()
    {
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
        // talk to host and receive a client ID
        yield return Setup();
        if (id == -1)
        {
            // failed to communicate with host
            throw new InvalidOperationException($"Failed to connect to host at {Host.udpSenderEp.ToString()}");
        }
        // receive host sync packets
        while (true)
        {
            List<UDPPacket> hostPackets = Host.Receive();
            foreach (UDPPacket packet in hostPackets)
            {
                IGamePacket gamePacket = GamePacketUtils.Deserialize(packet.data);
                if (gamePacket is SyncToClientPacket)
                {
                    // do things with host sync data
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
            Host.Send(GamePacketUtils.Serialize(new ClientSetupPacket() { ID = -1 }));
            yield return null;
            List<UDPPacket> packets = Host.Receive();
            foreach (UDPPacket packet in packets)
            {
                IGamePacket gamePacket = GamePacketUtils.Deserialize(packet.data);
                if (gamePacket is ClientSetupPacket)
                {
                    id = ((ClientSetupPacket)gamePacket).ID;
                    Debug.Log($"Connected to host at {packet.address.ToString()}:{packet.port}, we are Client #{id}");
                    Host.Send(GamePacketUtils.Serialize(gamePacket));
                    break;
                }
            }
        }
        yield break;
    }

    void OnApplicationQuit()
    {
        // kill hole puncher connection
        HolePuncher?.Kill();
        // kill connection with host
        Host?.Kill();
    }
}
