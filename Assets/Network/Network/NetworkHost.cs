using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkHost : MonoBehaviour, ISyncHost
{
    public NetManager netManager;
    EventBasedNetListener netEvent = new EventBasedNetListener();
    EventBasedNatPunchListener natPunchEvent = new EventBasedNatPunchListener();
    public NetPacketProcessor netPacketProcessor = new NetPacketProcessor();

    public NetworkObjectData data = new NetworkObjectData();
    // clients -> host
    public Dictionary<int, NetworkObjectData> clientData = new Dictionary<int, NetworkObjectData>();
    public Dictionary<int, NetworkMessages> clientMessages = new Dictionary<int, NetworkMessages>();

    static int currentObjectID = 1;
    public static int NextObjectID { get { return currentObjectID++; } }

    NetworkSync sync => GetComponent<NetworkSync>();

    [NonSerialized]
    public string ConnectionKey = "";

    [NonSerialized]
    public string RoomKey = "";

    [NonSerialized]
    public bool GameStarted;

    // peers
    [NonSerialized]
    public List<int> Peers = new List<int>();

    [NonSerialized]
    public NetworkObjects networkObjects;

    public void Initialize()
    {
        currentObjectID = 1;
        networkObjects = GetComponent<NetworkObjects>();
        // network handlers
        natPunchEvent.NatIntroductionSuccess += (point, addrType, token) =>
        {
            var peer = netManager.Connect(point, ConnectionKey);
            Debug.Log($"NatIntroductionSuccess. Connecting to client: {point}, type: {addrType}, connection created: {peer != null}");
        };
        netEvent.PeerConnectedEvent += peer => {
            Peers.Add(peer.Id);
            Debug.Log("PeerConnected: " + peer.EndPoint);
            CharacterMenu.Instance?.AddCharSelState(peer.Id);
            if (CharacterMenu.Instance != null)
            {
                Dictionary<int, int> peerIDsToPlayerIDs = CharacterMenu.Instance?.GetPeerIDsToPlayerIDs();
                foreach (int peerID in Peers)
                {
                    NetworkUtils.SendNetworkMessageToPeer(peer.Id, 0, new SetPlayerIDPacket()
                    {
                        PlayerID = peerIDsToPlayerIDs[peerID]
                    }, DeliveryMethod.ReliableOrdered);
                }
            }
            NetworkUtils.SendNetworkMessageToPeer(peer.Id, 0, new SceneChangePacket()
            {
                scene = "CharacterSelect",
                startingObjectID = 1
            }, DeliveryMethod.ReliableOrdered);
        };
        netEvent.ConnectionRequestEvent += request => { request.AcceptIfKey(ConnectionKey); };
        netEvent.NetworkReceiveEvent += (peer, reader, deliveryMethod) => {
            netPacketProcessor.ReadAllPackets(reader, peer);
        };
        netEvent.PeerDisconnectedEvent += (peer, disconnectInfo) => { Debug.Log($"Peer {peer} Disconnected: {disconnectInfo.Reason}"); };
        // packet handlers
        netPacketProcessor.SubscribeReusable<NetworkMessagePacket, NetPeer>((packet, peer) =>
        {
            if (!clientMessages.ContainsKey(peer.Id))
            {
                clientMessages[peer.Id] = new NetworkMessages();
            }
            clientMessages[peer.Id].SyncFromPacket(packet);
        });
        netPacketProcessor.SubscribeReusable<NetworkObjectDataPacket, NetPeer>((packet, peer) =>
        {
            if (!clientData.ContainsKey(peer.Id))
            {
                clientData[peer.Id] = new NetworkObjectData();
            }
            clientData[peer.Id].SyncFromPacket(packet);
        });
        // connect
        netManager = new NetManager(netEvent)
        {
            IPv6Enabled = IPv6Mode.Disabled,
            NatPunchEnabled = true
        };
        netManager.NatPunchModule.Init(natPunchEvent);
        netManager.Start();
        LoadObjectsInNewScene();
    }

    // update from network
    void FixedUpdate()
    {
        netManager.PollEvents();
        netManager.NatPunchModule.PollEvents();
        // send data packets
        netManager.SendToAll(netPacketProcessor.Write(data.ToPacket()), DeliveryMethod.Sequenced);
        // cleanup
        foreach (int peerID in clientMessages.Keys)
        {
            clientMessages[peerID].Update();
        }
    }

    public void SetScene(string scene)
    {
        StartCoroutine(SetSceneCoroutine(scene));
    }
    // coroutine for loading a scene
    IEnumerator SetSceneCoroutine(string scene)
    {
        // send scene change event to clients
        sync.SendNetworkMessage(new SceneChangePacket()
        {
            scene = scene,
            startingObjectID = currentObjectID
        }, DeliveryMethod.ReliableOrdered);
        // load scene
        SceneManager.LoadScene(scene);
        yield return null;
        LoadObjectsInNewScene();
        yield break;
    }
    // when scene loads, init all starting network objects
    void LoadObjectsInNewScene()
    {
        List<GameObject> startingEntities =
            GameObject.FindGameObjectWithTag("NetworkStartingEntities")?.GetComponent<NetworkStartingEntities>()?.startingEntities;
        if (startingEntities == null)
        {
            return;
        }
        foreach (GameObject obj in startingEntities)
        {
            if (obj == null)
            {
                continue;
            }
            NetworkObjects.RemoveIncorrectComponents(obj);
            NetworkSync sync = obj.GetComponent<NetworkSync>();
            if (obj == gameObject)
            {
                sync.Initialize(sync.ObjectID, sync.NetworkType);
                continue;
            }
            sync.Initialize(NextObjectID, sync.NetworkType);
            obj.SetActive(true);
        }
    }

    public GameObject CreateNetworkObject(string networkType)
    {
        GameObject obj = networkObjects.CreateNetworkObject(NextObjectID, networkType);
        return obj;
    }

    public GameObject CreateNetworkObject(string networkType, Vector3 position, Quaternion rotation)
    {
        GameObject obj = CreateNetworkObject(networkType);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    void OnApplicationQuit()
    {
        netManager.Stop();
    }
}

public class SetPlayerIDPacket : INetworkData
{
    public string Type { get; set; }
    public int PlayerID { get; set; }
}

public class SceneChangePacket : INetworkData
{
    public string Type { get; set; }
    public string scene { get; set; }
    public int startingObjectID { get; set; }
}

public class CreateNetworkObjectPacket : INetworkData
{
    public string Type { get; set; }
    public int objectID { get; set; }
    public string networkType { get; set; }
}

public class DestroyNetworkObjectPacket : INetworkData
{
    public string Type { get; set; }
    public int objectID { get; set; }
}
