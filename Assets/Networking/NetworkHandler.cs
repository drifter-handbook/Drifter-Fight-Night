using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkHandler
{
    private class PacketAction
    {
        public Action<int, IGamePacket> handler = null;
        public bool filterLatest = false;
        // player ID -> latest timestamp
        public Dictionary<int, float> latest;
        public PacketAction(Action<int, IGamePacket> handler, bool filterLatest)
        {
            this.handler = handler;
            this.filterLatest = filterLatest;
            if (this.filterLatest)
            {
                latest = new Dictionary<int, float>();
            }
        }
    }
    List<Action<IPAddress, int, int>> OnConnectHandlers = new List<Action<IPAddress, int, int>>();
    Dictionary<string, List<PacketAction>> OnReceiveHandlers = new Dictionary<string, List<PacketAction>>();
    List<Action> OnFailureHandlers = new List<Action>();

    private class P2PConnection
    {
        public int ID = -1;
        public IPAddress DestIP;
        public int DestPort;
    }
    Dictionary<int, P2PConnection> Connections = new Dictionary<int, P2PConnection>();

    public bool IsHost { get; private set; } = false;
    IPAddress hostIP;
    int hostID;
    public UDPHolePuncher HolePuncher;
    public int HolePunchID { get; private set; } = -1;
    UDPConnection Connection;

    public bool Active { get; private set; } = false;
    public bool Failed { get; private set; } = false;

    public NetworkHandler()
    {
        IsHost = true;
    }
    public NetworkHandler(string hostIP, int hostID)
    {
        IsHost = false;
        this.hostIP = IPAddress.Parse(hostIP.Trim());
        this.hostID = hostID;
    }

    // init
    public void Connect()
    {
        Active = true;
        HolePuncher = new UDPHolePuncher(hostIP == null ? "127.0.0.1" : hostIP.ToString(), "75.134.27.221", 6970, IsHost, hostID);
        HolePunchID = HolePuncher.ID;
        Connection = new UDPConnection(HolePuncher.udpClient);
    }

    public void Update()
    {
        if (Active)
        {
            // handle incoming connections
            foreach (P2PClient client in HolePuncher.ReceiveClients())
            {
                // add client
                int clientID = Connections.Count + 1;
                Connections[clientID] = new P2PConnection()
                {
                    ID = clientID,
                    DestIP = client.DestIP,
                    DestPort = client.DestPort
                };
                // run handlers
                foreach (Action<IPAddress, int, int> handler in OnConnectHandlers)
                {
                    handler.Invoke(client.DestIP, client.DestPort, clientID);
                }
            }
            // handle incoming packets
            foreach (UDPPacket udpPacket in Connection.Receive())
            {
                int connectionID = -1;
                foreach (P2PConnection conn in Connections.Values)
                {
                    if (conn.DestIP.ToString() == udpPacket.address.ToString() && conn.DestPort == udpPacket.port)
                    {
                        connectionID = conn.ID;
                    }
                }
                if (connectionID >= 0)
                {
                    IGamePacket packet = GamePacketUtils.Deserialize(udpPacket.data);
                    packet.address = udpPacket.address;
                    packet.port = udpPacket.port;
                    // run handlers
                    if (OnReceiveHandlers.ContainsKey(packet.TypeID))
                    {
                        foreach (PacketAction handler in OnReceiveHandlers[packet.TypeID])
                        {
                            // if only process latest packets of this type
                            if (handler.filterLatest)
                            {
                                if (!handler.latest.ContainsKey(connectionID))
                                {
                                    handler.latest[connectionID] = 0f;
                                }
                                if (handler.latest[connectionID] < packet.Timestamp)
                                {
                                    handler.latest[connectionID] = packet.Timestamp;
                                    handler.handler.Invoke(connectionID, packet);
                                }
                            }
                            // if process all packets of this type
                            else
                            {
                                handler.handler.Invoke(connectionID, packet);
                            }
                        }
                    }
                }
            }
            // handle failure
            if (HolePuncher.Failed)
            {
                Failed = true;
                Active = false;
            }
        }
    }

    public void StopAcceptingConnections()
    {
        HolePuncher.Kill();
    }

    // send packet using connectionID
    public void Send(int connectionID, IGamePacket packet)
    {
        if (Connections.ContainsKey(connectionID))
        {
            Connection.Send(Connections[connectionID].DestIP, Connections[connectionID].DestPort,
                GamePacketUtils.Serialize(packet));
        }
    }
    public void SendToAll(IGamePacket packet)
    {
        foreach (int connectionID in Connections.Keys)
        {
            Send(connectionID, packet);
        }
    }

    // add packet receive handler Action(ConnectionID)
    public void OnReceive(IGamePacket type, Action<int, IGamePacket> handler, bool filterLatest = false)
    {
        if (!OnReceiveHandlers.ContainsKey(type.TypeID))
        {
            OnReceiveHandlers[type.TypeID] = new List<PacketAction>();
        }
        if (OnReceiveHandlers[type.TypeID].Exists(x => x.handler == handler))
        {
            throw new InvalidOperationException("Handler is already registered.");
        }
        OnReceiveHandlers[type.TypeID].Add(new PacketAction(handler, filterLatest));
    }
    // remove packet receive handler Action(ConnectionID)
    public void RemoveOnReceiveHandler(IGamePacket type, Action<int, IGamePacket> handler, bool filterLatest)
    {
        if (!OnReceiveHandlers.ContainsKey(type.TypeID))
        {
            throw new InvalidOperationException("Handler does not exist.");
        }
        OnReceiveHandlers[type.TypeID].RemoveAll(x => x.handler == handler);
    }

    // add connect handler Action(DestAddress, DestPort, ConnectionID)
    public void OnConnect(Action<IPAddress, int, int> handler)
    {
        if (OnConnectHandlers.Exists(x => x == handler))
        {
            throw new InvalidOperationException("Handler is already registered.");
        }
        OnConnectHandlers.Add(handler);
    }
    // remove connect handler Action(DestAddress, DestPort, ConnectionID)
    public void RemoveOnConnectHandler(Action<IPAddress, int, int> handler)
    {
        if (!OnConnectHandlers.Contains(handler))
        {
            throw new InvalidOperationException("Handler does not exist.");
        }
        OnConnectHandlers.RemoveAll(x => x == handler);
    }

    // add failure handler Action()
    public void OnFailure(Action handler)
    {
        if (OnFailureHandlers.Exists(x => x == handler))
        {
            throw new InvalidOperationException("Handler is already registered.");
        }
        OnFailureHandlers.Add(handler);
    }
    // remove failure handler Action()
    public void RemoveOnFailureHandler(Action handler)
    {
        if (!OnFailureHandlers.Contains(handler))
        {
            throw new InvalidOperationException("Handler does not exist.");
        }
        OnFailureHandlers.RemoveAll(x => x == handler);
    }

    void OnDestroy()
    {
        // kill hole puncher connection
        HolePuncher?.Kill();
        // kill network connection
        Connection?.Kill();
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}
