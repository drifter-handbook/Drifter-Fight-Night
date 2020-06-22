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

    public UdpClient udpClient { get; private set; }
    object udpLock = new object();
    public IPEndPoint udpSenderEp { get; private set; }
    IPEndPoint udpReceiverSourceEp;

    bool sendOnly = false;

    public UDPConnection(UdpClient udpClient, IPAddress destIP, int destPort, bool sendOnly = false)
    {
        this.udpClient = udpClient;
        udpSenderEp = new IPEndPoint(destIP, destPort);
        if (!sendOnly)
        {
            received = new ConcurrentBag<UDPPacket>();
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
                lock(udpLock)
                {
                    if (udpClient != null && udpClient.Available > 0)
                    {
                        udpReceiverSourceEp = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = udpClient.Receive(ref udpReceiverSourceEp);
                        received.Add(new UDPPacket()
                        {
                            address = udpReceiverSourceEp.Address,
                            port = udpReceiverSourceEp.Port,
                            data = data
                        });
                    }
                }
            }
            catch (SocketException)
            {
            }
        }
        lock(udpLock)
        {
            udpClient.Close();
        }
    }

    // stop the thread
    public void Kill()
    {
        killed = true;
    }

    // send a packet
    public void Send(byte[] data)
    {
        lock (udpLock)
        {
            udpClient.Send(data, data.Length, udpSenderEp);
        }
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