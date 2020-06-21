using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    // -1 id for unassigned
    public int id { get; private set; } = -1;

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
                Debug.Log($"Connected to host at {Host.udpSenderEp.ToString()}");
            }
            if (HolePuncher.Failed)
            {
                Debug.Log($"Failed to connect to server {HolePuncher.holePunchingServerName}:{HolePuncher.holePunchingServerPort}");
                yield break;
            }
            yield return null;
        }
        // setup setup to server
        Debug.Log($"Sending pings to host at {Host.udpSenderEp.ToString()}");
        for (int i = 0; i < 5; i++)
        {
            Host.Send(Encoding.ASCII.GetBytes("Ping"));
            yield return new WaitForSeconds(0.1f);
        }
        // pong clients
        List<UDPPacket> packets = Host.Receive();
        foreach (UDPPacket packet in packets)
        {
            if (Encoding.ASCII.GetString(packet.data) == "Ping")
            {
                Debug.Log($"Ping received from host at {packet.address.ToString()}:{packet.port}");
                Host.Send(Encoding.ASCII.GetBytes("Pong"));
                break;
            }
        }
        yield return new WaitForSeconds(0.1f);
        // receive pongs on server
        packets = Host.Receive();
        foreach (UDPPacket packet in packets)
        {
            if (Encoding.ASCII.GetString(packet.data) == "Ping")
            {
                Debug.Log($"Pong received from host at {packet.address.ToString()}:{packet.port}");
                break;
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

    void OnApplicationQuit()
    {
        // kill hole puncher connection
        HolePuncher?.Kill();
        // kill connection with host
        Host?.Kill();
    }
}
