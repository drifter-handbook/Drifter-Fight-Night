using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPNetwork
{
    public bool Connecting { get; private set; } = false;
    public bool Connected { get; private set; } = false;
    public bool Failed { get; private set; } = false;

    IPAddress destAddress;
    string holePunchingServerName;
    int holePunchingServerPort;

    Thread thread;
    bool killed;
    ConcurrentBag<byte[]> received;

    UdpClient senderClient;
    UdpClient receiverClient;
    IPEndPoint udpSenderEp;
    IPEndPoint udpReceiverSourceEp;

    HolePunchResponse response;

    private class HolePunchRequest
    {
        public string ip = "";
    }
    private class HolePunchResponse
    {
        public int source_port = 0;
        public string source_ip = "";
        public int dest_port = 0;
        public string dest_ip = "";
        public string error = "";
    }

    public UDPNetwork(string destName, string holePunchingServerName, int holePunchingServerPort)
    {
        destAddress = IPAddress.Parse(destName);
        this.holePunchingServerName = holePunchingServerName;
        this.holePunchingServerPort = holePunchingServerPort;
        senderClient = new UdpClient();
        received = new ConcurrentBag<byte[]>();
    }

    // connect to the other client via hole-punching
    IEnumerator ConnectToDestination()
    {
        // get hole punch server response
        response = null;
        thread = new Thread(new ThreadStart(TalkToHolepunchServer));
        for (float time = 0; time < 3f && response == null; time += Time.deltaTime)
        {
            yield return null;
        }
        if (response == null)
        {
            Connecting = false;
            Failed = true;
            Debug.Log("Failed to communicate with hole punch server.");
            yield break;
        }
        Debug.Log(JsonConvert.SerializeObject(response));

        // connect to destination
        udpSenderEp = new IPEndPoint(IPAddress.Parse(response.dest_ip), response.dest_port);
        const int tries = 5;
        Debug.Log("Spam data to open the UDP ports");
        for (int i = 0; i < tries && !killed; i++)
        {
            // send packets at their router-mapped port
            Send(Encoding.ASCII.GetBytes("Establish"));
            yield return new WaitForSeconds(0.1f);
        }

        // perform UDP handshake
        receiverClient = new UdpClient(response.source_port, IPAddress.Parse(response.source_ip).AddressFamily);
        bool sendCheck = false;
        bool recvCheck = false;
        for (int p = 0; p < 90 && !Connected && !killed; p++)
        {
            // check sending/receiving packets
            if (!sendCheck)
            {
                // send "Ping"
                Send(Encoding.ASCII.GetBytes("Ping"));
            }
            List<byte[]> receivedPackets = Receive();
            if (receivedPackets.Count > 0)
            {
                foreach (byte[] packet in receivedPackets)
                {
                    string pong = Encoding.ASCII.GetString(packet);
                    // if we receive Pong, we know we can send packets
                    if (pong == "Pong")
                    {
                        sendCheck = true;
                    }
                    // if we recieve Ping, we know we can receive packets
                    if (pong == "Ping")
                    {
                        recvCheck = true;
                        Send(Encoding.ASCII.GetBytes("Pong"));
                    }
                }
            }
            Connected = sendCheck && recvCheck;
            yield return new WaitForSeconds(0.1f);
        }
        // finish
        Connecting = false;
        Debug.Log($"CONNECTED: {Connected}");
        Failed = !Connected;
    }

    private void TalkToHolepunchServer()
    {
        byte[] data;
        // send UDP to hole punch server to give it your assigned port
        data = Encoding.ASCII.GetBytes("Setup");
        senderClient.Send(data, data.Length, holePunchingServerName, holePunchingServerPort);
        Thread.Sleep(100);

        // open TCP connection to hole punch server to give it our destination
        string req = JsonConvert.SerializeObject(new HolePunchRequest() { ip = destAddress.ToString() });
        TcpClient tcpClient = new TcpClient();
        tcpClient.Connect(holePunchingServerName, holePunchingServerPort);
        data = Encoding.Default.GetBytes(req);
        NetworkStream stream = tcpClient.GetStream();
        stream.Write(data, 0, data.Length);

        // receive hole punch server response
        data = new byte[4096];
        int readBytes = stream.Read(data, 0, data.Length);
        stream.Close();
        tcpClient.Close();

        // parse response
        string resp = Encoding.ASCII.GetString(data, 0, readBytes);
        HolePunchResponse r = JsonConvert.DeserializeObject<HolePunchResponse>(resp);
        if (!(r.source_ip == "" || r.dest_ip == "" || r.source_port == 0 || r.dest_port == 0 || r.error != ""))
        {
            response = r;
        }
    }

    private void ReceiveData()
    {
        while (!killed)
        {
            try
            {
                if (receiverClient != null && receiverClient.Available > 0)
                {
                    udpReceiverSourceEp = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = receiverClient.Receive(ref udpReceiverSourceEp);
                    received.Add(data);
                }
            }
            catch (SocketException e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    // connect to the destination and start receiving data
    public IEnumerator Connect()
    {
        if (Connecting || Connected || Failed)
        {
            throw new InvalidOperationException("Already attempted to connect.");
        }
        Connecting = true;
        // start receiving data
        thread = new Thread(new ThreadStart(ReceiveData));
        thread.Start();
        // attempt to connect
        yield return ConnectToDestination();
    }

    // stop the thread
    public void Kill()
    {
        killed = true;
    }

    // send a packet
    public void Send(byte[] data)
    {
        senderClient.Send(data, data.Length, udpSenderEp);
    }

    // get all packets received
    public List<byte[]> Receive()
    {
        List<byte[]> packets = new List<byte[]>();
        byte[] data;
        while (!received.IsEmpty && received.TryTake(out data))
        {
            packets.Add(data);
        }
        return packets;
    }
}
