using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkHandler : MonoBehaviour
{
    private class PacketAction
    {
        public Action handler = null;
        public bool filterLatest = false;
        // player ID -> latest timestamp
        public Dictionary<int, float> latest;
        public PacketAction(Action handler, bool filterLatest)
        {
            this.handler = handler;
            this.filterLatest = filterLatest;
            if (this.filterLatest)
            {
                latest = new Dictionary<int, float>();
            }
        }
    }
    Dictionary<string, List<PacketAction>> OnReceiveHandlers = new Dictionary<string, List<PacketAction>>();

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnReceive(IGamePacket type, Action handler, bool filterLatest)
    {
        if (!OnReceiveHandlers.ContainsKey(type.TypeID))
        {
            OnReceiveHandlers[type.TypeID] = new List<PacketAction>();
        }

    }
}
