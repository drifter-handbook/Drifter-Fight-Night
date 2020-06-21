using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UDPHolePuncher : IDisposable
{
    IPAddress destAddress;
    string holePunchingServerName;
    int holePunchingServerPort;
    bool host;

    List<P2PClient> clients;

    Thread thread;
    bool killed = false;
    ConcurrentBag<P2PClient> received;

    private class HolePunchRequest
    {
        public string IP = "";
        public bool Persistent = false;
    }
    public class P2PClient
    {
        public int SourcePort = 0;
        public string SourceIP = "";
        public int DestPort = 0;
        public string DestIP = "";
        public string Error = "";
    }

    public UDPHolePuncher(string destName, string holePunchingServerName, int holePunchingServerPort, bool host)
    {
        destAddress = IPAddress.Parse(destName);
        this.holePunchingServerName = holePunchingServerName;
        this.holePunchingServerPort = holePunchingServerPort;
        this.host = host;
        received = new ConcurrentBag<P2PClient>();
        thread = new Thread(new ThreadStart(ReceiveClientData));
        thread.Start();
    }

    private void ReceiveClientData()
    {
        byte[] data;
        // send UDP to hole punch server to give it your assigned port
        UdpClient udpClient = new UdpClient();
        data = Encoding.ASCII.GetBytes("Setup");
        udpClient.Send(data, data.Length, holePunchingServerName, holePunchingServerPort);
        Thread.Sleep(100);

        // open TCP connection to hole punch server to give it our destination
        string req = JsonConvert.SerializeObject(new HolePunchRequest() { IP = destAddress.ToString(), Persistent = host });
        TcpClient tcpClient = new TcpClient();
        tcpClient.Connect(holePunchingServerName, holePunchingServerPort);
        data = Encoding.Default.GetBytes(req);
        NetworkStream stream = tcpClient.GetStream();
        stream.Write(data, 0, data.Length);

        // receive hole punch server responses
        string s = "";
        while (!killed && stream.DataAvailable)
        {
            data = new byte[4096];
            int readBytes = stream.Read(data, 0, data.Length);
            // parse response
            s += Encoding.ASCII.GetString(data, 0, readBytes);
            const int MAX_CLIENTS_PER_PACKET = 10;
            for (int i = 0; i < MAX_CLIENTS_PER_PACKET && s.Contains("\n"); i++)
            {
                string[] split = s.Split(new[] {"\n"}, 1, StringSplitOptions.RemoveEmptyEntries);
                P2PClient r = JsonConvert.DeserializeObject<P2PClient>(split[0]);
                // validate and add to received clients
                received.Add(r);
                // terminate connection if we are not a host, can only receive one server connection
                if (!host)
                {
                    Kill();
                    i = MAX_CLIENTS_PER_PACKET;
                }
                s = split[1];
            }
        }
        stream.Close();
        tcpClient.Close();
    }

    // get all packets received
    public List<P2PClient> ReceiveClients()
    {
        List<P2PClient> clients = new List<P2PClient>();
        P2PClient client;
        while (!received.IsEmpty && received.TryTake(out client))
        {
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
