using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : MonoBehaviour, ISyncClient, INetworkMessageReceiver
{
    public NetManager netManager;
    EventBasedNetListener netEvent = new EventBasedNetListener();
    EventBasedNatPunchListener natPunchEvent = new EventBasedNatPunchListener();
    public NetPacketProcessor netPacketProcessor = new NetPacketProcessor();

    // host -> client
    public NetworkObjectData data = new NetworkObjectData();
    public NetworkObjectData dataToHost = new NetworkObjectData();
    public NetworkMessages messages = new NetworkMessages();

    [NonSerialized]
    public NetworkObjects networkObjects;

    [NonSerialized]
    public string ConnectionKey = "";

    [NonSerialized]
    public bool GameStarted;

    public void Initialize()
    {
        networkObjects = GetComponent<NetworkObjects>();
        // network handlers
        natPunchEvent.NatIntroductionSuccess += (point, addrType, token) =>
        {
            var peer = netManager.Connect(point, ConnectionKey);
            Debug.Log($"NatIntroductionSuccess. Connecting to peer: {point}, type: {addrType}, connection created: {peer != null}");
        };
        netEvent.PeerConnectedEvent += peer => { Debug.Log("PeerConnected: " + peer.EndPoint); };
        netEvent.ConnectionRequestEvent += request => { request.AcceptIfKey(ConnectionKey); };
        netEvent.NetworkReceiveEvent += (peer, reader, deliveryMethod) => {
            netPacketProcessor.ReadAllPackets(reader, peer);
        };
        netEvent.PeerDisconnectedEvent += (peer, disconnectInfo) => { Debug.Log($"Peer {peer} Disconnected: {disconnectInfo.Reason}"); };
        // packet handlers
        netPacketProcessor.SubscribeReusable<NetworkMessagePacket, NetPeer>((packet, peer) =>
        {
            messages.SyncFromPacket(packet);
        });
        netPacketProcessor.SubscribeReusable<NetworkObjectDataPacket, NetPeer>((packet, peer) =>
        {
            data.SyncFromPacket(packet);
        });
        // connect
        netManager = new NetManager(netEvent)
        {
            IPv6Enabled = IPv6Mode.Disabled,
            NatPunchEnabled = true
        };
        netManager.NatPunchModule.Init(natPunchEvent);
        netManager.Start();
        LoadObjectsInNewScene(0);
    }

    // update from network
    void FixedUpdate()
    {
        netManager.PollEvents();
        netManager.NatPunchModule.PollEvents();
        // send data packets
        netManager.SendToAll(netPacketProcessor.Write(dataToHost.ToPacket()), DeliveryMethod.Sequenced);
        // cleanup
        messages.Update();
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        SetPlayerIDPacket setPlayerID = NetworkUtils.GetNetworkData<SetPlayerIDPacket>(message.contents);
        if (setPlayerID != null)
        {
            Debug.Log($"We are now Player #{setPlayerID.PlayerID}.");
            GameController.Instance.PlayerID = setPlayerID.PlayerID;
            return;
        }
        SceneChangePacket sceneChange = NetworkUtils.GetNetworkData<SceneChangePacket>(message.contents);
        if (sceneChange != null)
        {
            Debug.Log($"Scene change received: {sceneChange.scene}: {sceneChange.startingObjectID}.");
            SetScene(sceneChange.scene, sceneChange.startingObjectID);
            return;
        }
        CreateNetworkObjectPacket createObject = NetworkUtils.GetNetworkData<CreateNetworkObjectPacket>(message.contents);
        if (createObject != null)
        {
            networkObjects.CreateNetworkObject(createObject.objectID, createObject.networkType);
            return;
        }
        DestroyNetworkObjectPacket destroyObject = NetworkUtils.GetNetworkData<DestroyNetworkObjectPacket>(message.contents);
        if (destroyObject != null)
        {
            networkObjects.DestroyNetworkObject(destroyObject.objectID);
            return;
        }
    }

    public void SetScene(string scene, int sceneStartingObjectID)
    {
        StartCoroutine(SetSceneCoroutine(scene, sceneStartingObjectID));
    }
    // coroutine for loading a scene
    IEnumerator SetSceneCoroutine(string scene, int sceneStartingObjectID)
    {
        SceneManager.LoadScene(scene);
        yield return null;
        LoadObjectsInNewScene(sceneStartingObjectID);
        yield break;
    }
    // when scene loads, init all starting network objects
    void LoadObjectsInNewScene(int sceneStartingObjectID)
    {
        List<GameObject> startingEntities =
            GameObject.FindGameObjectWithTag("NetworkStartingEntities")?.GetComponent<NetworkStartingEntities>()?.startingEntities;
        if (startingEntities == null)
        {
            return;
        }
        for (int i = 0; i < startingEntities.Count; i++)
        {
            GameObject obj = startingEntities[i];
            // GameController doesn't follow the same rules
            NetworkObjects.RemoveIncorrectComponents(obj);
            NetworkSync sync = obj.GetComponent<NetworkSync>();
            if (string.IsNullOrWhiteSpace(sync.NetworkType))
            {
                throw new InvalidTypeException($"NetworkType field is empty in starting network object {obj.name}");
            }
            if (obj == gameObject)
            {
                sync.Initialize(sync.ObjectID, sync.NetworkType);
                continue;
            }
            sync.Initialize(sceneStartingObjectID + i, sync.NetworkType);
            obj.SetActive(false);
        }
        StartCoroutine(LoadObjectsCoroutine(startingEntities));
    }
    IEnumerator LoadObjectsCoroutine(List<GameObject> startingEntities)
    {
        while (startingEntities.Any(x => !x.activeSelf))
        {
            // check if we have received data. If so, activate object
            foreach (GameObject obj in startingEntities)
            {
                if (NetworkUtils.GetNetworkObjectData(obj.GetComponent<NetworkSync>().ObjectID).Count > 0)
                {
                    obj.SetActive(true);
                }
            }
            yield return null;
        }
    }

    void OnApplicationQuit()
    {
        netManager.Stop();
    }
}
