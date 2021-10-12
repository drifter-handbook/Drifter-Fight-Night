using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NetworkObjects : MonoBehaviour
{
    public List<GameObject> NetworkTypePrefabs = new List<GameObject>();

    // currently existing network objects
    Dictionary<int, GameObject> networkObjects = new Dictionary<int, GameObject>();
    // objects where CreateNetworkObject came in before the data
    Dictionary<int, string> createQueue = new Dictionary<int, string>();
    // objects where DestroyNetworkObject came in before CreateNetworkObject somehow
    Dictionary<int, bool> destroyQueue = new Dictionary<int, bool>();

    // Start is called before the first frame update
    void Start()
    {

        //UnityEngine.Debug.Log(Directory.GetCurrentDirectory());
        aggregatePrefabs("Assets/Resources/");
    }

    // Update is called once per frame
    void Update()
    {
        // if object that is supposed to be created has not yet been, create it
        Dictionary<int, string> createObjects = new Dictionary<int, string>(createQueue);
        foreach (int objectID in createObjects.Keys)
        {
            if (NetworkUtils.GetNetworkObjectData(objectID).Count > 0)
            {
                CreateNetworkObjectForReal(objectID, createObjects[objectID]);
                createQueue.Remove(objectID);
            }
        }
        // if object that is supposed to be destroyed now exist, destroy it
        Dictionary<int, bool> destroyObjects = new Dictionary<int, bool>(destroyQueue);
        foreach (int objectID in destroyObjects.Keys)
        {
            if (networkObjects.ContainsKey(objectID))
            {
                DestroyNetworkObjectForReal(objectID);
                destroyQueue.Remove(objectID);
            }
        }
    }

    GameObject GetNetworkTypePrefab(string networkType)
    {
        return NetworkTypePrefabs.Find(x => x.name == networkType);
    }

    public GameObject CreateNetworkObject(int objectID, string networkType)
    {
        if (GameController.Instance.IsHost || NetworkUtils.GetNetworkObjectData(objectID).Count > 0)
        {
            return CreateNetworkObjectForReal(objectID, networkType);
        }
        // It is possible for a create request to come in before its initial data.
        // If so, queue the creation until its initial data is received.
        // This can't memory leak, since object creation is sent reliably.
        // So, unless the creation packet NEVER arrives (aka we lost connection so it doesn't matter),
        // then we won't leak memory
        else
        {
            createQueue[objectID] = networkType;
        }
        return null;
    }
    public static void RemoveIncorrectComponents(GameObject networkObj)
    {
        // remove components of wrong type
        if(networkObj == null)networkObj = GameController.Instance.gameObject;

        if (GameController.Instance.IsHost)
        {
            foreach (ISyncClient script in networkObj.GetComponents<ISyncClient>())
            {
                Destroy((MonoBehaviour)script);
            }
        }
        else
        {
            foreach (ISyncHost script in networkObj.GetComponents<ISyncHost>())
            {
                Destroy((MonoBehaviour)script);
            }
        }
    }
    GameObject CreateNetworkObjectForReal(int objectID, string networkType)
    {
        if (!GameController.Instance.IsHost)
        {
            NetworkClient.currentObjectID = objectID;
            objectID = NetworkClient.NextObjectID;
        }
        GameObject networkObj = Instantiate(GetNetworkTypePrefab(networkType));

        //UnityEngine.Debug.Log(networkObjects)
        RegisterNetworkObject(objectID, networkType, networkObj);
        if (GameController.Instance.IsHost)
        {
            NetworkUtils.SendNetworkMessage(0, new CreateNetworkObjectPacket()
            {
                objectID = objectID,
                networkType = networkType
            });
        }
        return networkObj;
    }
    public void RegisterNetworkObject(int objectID, string networkType, GameObject networkObj)
    {
        if (networkObjects.ContainsKey(objectID))
        {
            if(networkObjects[objectID] == null)
                networkObjects.Remove(objectID);
            else
                throw new InvalidOperationException($"Object ID {objectID} already exists. GameObjects: {networkObjects[objectID].name}, {networkObj.name}");
        }
        RemoveIncorrectComponents(networkObj);
        // initialize
        NetworkSync sync = networkObj.GetComponent<NetworkSync>();
        if (sync == null)
        {
            throw new MissingComponentException($"NetworkSync component is required on Network Object {networkObj.name}");
        }
        sync.Initialize(objectID, networkType);
        // track created network objects
        networkObjects[objectID] = networkObj;
    }

    // these should be only called by client
    public void DestroyNetworkObject(int objectID)
    {
        if (networkObjects.ContainsKey(objectID))
        {
            DestroyNetworkObjectForReal(objectID);
        }
        // It is possible for a destroy request to come in before a create request.
        // If so, queue the destruction until it's created.
        // This can't memory leak, since object creation is sent reliably.
        // So, unless the creation packet NEVER arrives (aka we lost connection so it doesn't matter),
        // then we won't leak memory
        else
        {
            destroyQueue[objectID] = true;
        }
    }
    void DestroyNetworkObjectForReal(int objectID)
    {
        Destroy(networkObjects[objectID]);
        networkObjects.Remove(objectID);
    }

    // this should only be called by host
    public void RemoveNetworkObjectEntry(int objectID)
    {
        networkObjects.Remove(objectID);
    }


    //Populates the Network Prefabs list in Lucille Johnson
    private void aggregatePrefabs(string basePath)
    {

        //string[] networkPrefabs = Directory.GetFiles(basePath,"*.prefab",SearchOption.AllDirectories);

        UnityEngine.Object[] networkPrefabs = Resources.LoadAll("", typeof(GameObject));

        for(int i = 0; i < networkPrefabs.Length; i++)

           NetworkTypePrefabs.Add((GameObject)networkPrefabs[i]);

        UnityEngine.Debug.Log("Added " + NetworkTypePrefabs.Count + " Prefabs to the Network Prefab List");

    }
}
