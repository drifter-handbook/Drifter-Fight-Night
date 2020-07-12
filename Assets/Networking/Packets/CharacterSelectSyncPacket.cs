﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public class CharacterSelectState
{
    public int x = 0;
    public int y = 0;
    public bool locked = false;
}

public class CharacterSelectSyncPacket : IGamePacket
{
    public IPAddress address { get; set; }
    public int port { get; set; }

    public string TypeID { get; set; } = "CSS"; // get rekt noah
    public float Timestamp { get; set; }

    public CharacterSelectSyncData Data;
    public class CharacterSelectSyncData
    {
        public List<CharacterSelectState> CharacterSelectState = new List<CharacterSelectState>();
    }

    public IGamePacket FromData(string json)
    {
        CharacterSelectSyncData data = JsonConvert.DeserializeObject<CharacterSelectSyncData>(json);
        return new CharacterSelectSyncPacket() { Data = data };
    }

    public object ToData()
    {
        return Data;
    }
}