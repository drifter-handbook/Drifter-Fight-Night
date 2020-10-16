using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using UnityEngine;

public interface IGamePacket
{
    IPAddress address { get; set; }
    int port { get; set; }

    // unique packet identifier
    string TypeID { get; }

    // timestamp
    float Timestamp { get; set; }

    // create a data object that represents this packet
    object ToData();

    // create a new game packet with this data
    IGamePacket FromData(string json);
}