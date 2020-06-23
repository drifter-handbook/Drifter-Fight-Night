using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class P2PClient
{
    public UdpClient UdpClient;
    public IPAddress DestIP;
    public int DestPort;
}

public class UDPHolePuncher : IDisposable
{
    static Random random = new Random();
    int id = -1;

    UdpClient udpClient;
    IPAddress destAddress;
    public string holePunchingServerName { get; private set; }
    public int holePunchingServerPort { get; private set; }
    bool host;

    Thread thread;
    bool killed = false;
    ConcurrentBag<HolePunchResponse> received;

    public bool Failed { get; private set; } = false;
    
    private class HolePunchID
    {
        public int PeerPort = -1;
    }
    private class HolePunchRequest
    {
        public string RemoteIP = "";
        public int RemotePeerPort = -1;
        public int LocalPeerPort = -1;
        public string ConnectionType = "";
    }
    public class HolePunchResponse
    {
        public int SourcePort = 0;
        public string SourceIP = "";
        public int DestPort = 0;
        public string DestIP = "";
        public string Error = "";
    }

    public UDPHolePuncher(string destName, string holePunchingServerName, int holePunchingServerPort, bool host)
    {
        id = random.Next();
        destAddress = IPAddress.Parse(destName);
        this.holePunchingServerName = holePunchingServerName;
        this.holePunchingServerPort = holePunchingServerPort;
        this.host = host;
        received = new ConcurrentBag<HolePunchResponse>();
        // send UDP to hole punch server to give it your assigned port
        udpClient = new UdpClient();
        byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new HolePunchID() { PeerPort = host ? 0 : 1 }));
        udpClient.Send(data, data.Length, holePunchingServerName, holePunchingServerPort);
        // start receive thread
        thread = new Thread(new ThreadStart(ReceiveClientData));
        thread.Start();
    }

    private void ReceiveClientData()
    {
        // open TCP connection to hole punch server to give it our destination
        HolePunchRequest rq = new HolePunchRequest()
        {
            LocalPeerPort = host ? 0 : 1,
            RemoteIP = destAddress.ToString(),
            RemotePeerPort = host ? 1 : 0,
            ConnectionType = host ? "Server" : "Client"
        };
        string req = JsonConvert.SerializeObject(rq);
        TcpClient tcpClient = new TcpClient();
        try
        {
            tcpClient.Connect(holePunchingServerName, holePunchingServerPort);
        }
        catch (SocketException)
        {
            Failed = true;
            return;
        }
        byte[] data = Encoding.Default.GetBytes(req);
        NetworkStream stream = tcpClient.GetStream();
        stream.Write(data, 0, data.Length);

        // receive hole punch server responses
        string s = "";
        float time = 0;
        while (!killed)
        {
            const float SERVER_REFRESH_TIME = 10f;
            if (time > SERVER_REFRESH_TIME)
            {
                rq.ConnectionType = "KeepAlive";
                data = Encoding.Default.GetBytes(JsonConvert.SerializeObject(rq));
                stream.Write(data, 0, data.Length);
            }
            if (!stream.DataAvailable)
            {
                Thread.Sleep(50);
                time += 50f / 1000;
                continue;
            }
            data = new byte[4096];
            int readBytes = stream.Read(data, 0, data.Length);
            // parse response
            s += Encoding.ASCII.GetString(data, 0, readBytes).Replace("}", "}\n");
            const int MAX_CLIENTS_PER_PACKET = 10;
            for (int i = 0; i < MAX_CLIENTS_PER_PACKET && s.Contains("\n"); i++)
            {
                string[] split = s.Split(new[] {"\n"}, 1, StringSplitOptions.None);
                HolePunchResponse r = JsonConvert.DeserializeObject<HolePunchResponse>(split[0]);
                // validate and add to received clients
                if (r.Error == "")
                {
                    received.Add(r);
                    // terminate connection if we are not a host, can only receive one server connection
                    if (!host)
                    {
                        Kill();
                        i = MAX_CLIENTS_PER_PACKET;
                    }
                    // remove processed json
                    s = "";
                    if (split.Length > 1)
                    {
                        s = split[1];
                    }
                }
            }
        }
        stream.Close();
        tcpClient.Close();
    }

    // get all packets received
    public List<P2PClient> ReceiveClients()
    {
        List<P2PClient> clients = new List<P2PClient>();
        HolePunchResponse resp;
        while (!received.IsEmpty && received.TryTake(out resp))
        {
            P2PClient client = new P2PClient() { UdpClient = udpClient, DestIP = IPAddress.Parse(resp.DestIP), DestPort = resp.DestPort };
            byte[] noop = GamePacketUtils.Serialize(new NoOpPacket());
            client.UdpClient.Send(noop, noop.Length, new IPEndPoint(client.DestIP, client.DestPort));
            clients.Add(client);
        }
        return clients;
    }

    public void Kill()
    {
        killed = true;
    }

    public void Dispose()
    {
        Kill();
    }
}
