﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPPacket
{
    public IPAddress address;
    public int port;
    public byte[] data;
}

public class UDPConnection : IDisposable
{
    Thread thread;
    bool killed;
    ConcurrentBag<UDPPacket> received;

    UdpClient senderClient;
    UdpClient receiverClient;
    public IPEndPoint udpSenderEp { get; private set; }
    IPEndPoint udpReceiverSourceEp;

    bool sendOnly = false;

    public UDPConnection(string sourceIP, int sourcePort, string destIP, int destPort, bool sendOnly = false)
    {
        senderClient = new UdpClient();
        udpSenderEp = new IPEndPoint(IPAddress.Parse(destIP), destPort);
        if (!sendOnly)
        {
            received = new ConcurrentBag<UDPPacket>();
            receiverClient = new UdpClient(sourcePort, IPAddress.Parse(sourceIP).AddressFamily);
            thread = new Thread(new ThreadStart(ReceiveData));
            thread.Start();
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
                    received.Add(new UDPPacket() {
                        address = udpReceiverSourceEp.Address,
                        port = udpReceiverSourceEp.Port,
                        data = data
                    });
                }
            }
            catch (SocketException e)
            {
                Debug.Log(e.ToString());
            }
        }
        senderClient.Close();
        receiverClient.Close();
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
    public List<UDPPacket> Receive()
    {
        if (sendOnly)
        {
            throw new InvalidOperationException("Cannot receive data on a send only connection.");
        }
        List<UDPPacket> packets = new List<UDPPacket>();
        UDPPacket packet;
        while (!received.IsEmpty && received.TryTake(out packet))
        {
            packets.Add(packet);
        }
        return packets;
    }

    public void Dispose()
    {
        Kill();
    }
}