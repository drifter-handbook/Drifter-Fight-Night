using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkPingTracker
{
    public const float PING_INTERVAL = 2f;
    public const float DISCONNECT_TIMEOUT = 30f;

    private class ConnectionPingTracker
    {
        public float latestPingSent = 0f;
        public float latestPingResponse = 0f;
        public float latestPing = -1f;
    }
    // Connection ID -> Ping Tracker
    Dictionary<int, ConnectionPingTracker> pingTargets = new Dictionary<int, ConnectionPingTracker>();

    List<Action<int>> handlers = new List<Action<int>>();

    public NetworkPingTracker(NetworkHandler network, NetworkTimer timer)
    {
        // register
        network.OnConnect((addr, port, id) =>
        {
            pingTargets[id] = new ConnectionPingTracker()
            {
                latestPingSent = Time.time,
                latestPingResponse = Time.time
            };
        });
        // send pings
        timer.Schedule(() =>
        {
            network.SendToAll(new PingPacket() { PingTimestamp = Time.time, Response = false });
            // check if any connections timed out
            List<int> timedOut = new List<int>();
            foreach (int id in pingTargets.Keys)
            {
                pingTargets[id].latestPingSent = Time.time;
                if (Time.time - pingTargets[id].latestPingResponse > DISCONNECT_TIMEOUT)
                {
                    foreach (Action<int> handler in handlers)
                    {
                        handler.Invoke(id);
                    }
                    timedOut.Add(id);
                }
            }
            foreach (int id in timedOut)
            {
                pingTargets.Remove(id);
            }
        }, PING_INTERVAL);
        // receive pings
        network.OnReceive(new PingPacket(), (id, packet) =>
        {
            PingPacket ping = (PingPacket)packet;
            // if receiving a request for a ping from someone, send it back
            if (!ping.Response)
            {
                ping.Response = true;
                network.Send(id, ping);
            }
            // if receiving a response to one of our pings, track it
            else if (pingTargets.ContainsKey(id))
            {
                if (Mathf.Abs(ping.PingTimestamp - pingTargets[id].latestPingSent) < 0.01f)
                {
                    pingTargets[id].latestPing = Time.time - pingTargets[id].latestPingSent;
                    pingTargets[id].latestPingResponse = Time.time;
                }
            }
            pingTargets[id].latestPingResponse = Time.time;
        });
    }

    public float GetPing()
    {
        return GetPing(pingTargets.Keys.First());
    }

    public float GetPing(int id)
    {
        return pingTargets[id].latestPing;
    }

    // add timer handler Action(id)
    public void OnDisconnect(Action<int> handler)
    {
        if (handlers.Exists(x => x == handler))
        {
            throw new InvalidOperationException("Handler is already registered.");
        }
        handlers.Add(handler);
    }

    // remove action
    public void RemoveOnDisconnectHandler(Action<int> handler)
    {
        if (!handlers.Exists(x => x == handler))
        {
            throw new InvalidOperationException("Handler does not exist.");
        }
        handlers.RemoveAll(x => x == handler);
    }
}
