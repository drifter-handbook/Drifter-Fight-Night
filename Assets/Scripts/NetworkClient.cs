using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    // -1 id for unassigned
    public int id { get; private set; } = -1;

    public UDPConnection Host { get; private set; }
    public UDPHolePuncher HolePuncher { get; private set; }

    Coroutine coroutine;

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
            List<UDPHolePuncher.P2PClient> hosts = HolePuncher.ReceiveClients();
            if (hosts.Count > 0)
            {
                Host = new UDPConnection(hosts[0].SourceIP, hosts[0].SourcePort, hosts[0].DestIP, hosts[0].DestPort);
            }
            yield return null;
        }
        // setup client

        // run
        while (true)
        {
            yield return null;
        }
    }

    void OnApplicationQuit()
    {
        // kill hole puncher connection
        HolePuncher.Kill();
        // kill connection with host
        Host.Kill();
    }
}
