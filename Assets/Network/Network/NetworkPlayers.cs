using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayers : MonoBehaviour, ISyncHost
{
    NetworkSyncToHost syncFromClients;

    public List<GameObject> spawnPoints;

    GameObject hostPlayer;
    Dictionary<int, GameObject> clientPlayers = new Dictionary<int, GameObject>();

    [NonSerialized]
    public Dictionary<int,GameObject> players = new Dictionary<int,GameObject>();

    public static NetworkPlayers Instance => GameObject.FindGameObjectWithTag("NetworkPlayers")?.GetComponent<NetworkPlayers>();

    // Start is called before the first frame update
    void Start()
    {
        //UnityEngine.Debug.Log(GameController.Instance.host.Peers.Count);
        syncFromClients = GetComponent<NetworkSyncToHost>();
        // create host
        int playerNum = 0;
        hostPlayer = CreatePlayer(-1, ref playerNum);
        // create other players
        foreach (int peerID in GameController.Instance.host.Peers)
        {
            clientPlayers[peerID] = CreatePlayer(peerID, ref playerNum);
        }
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInputData input = GetInput(GameController.Instance.controls);
        UpdateInput(hostPlayer, input);
        foreach (int peerID in GameController.Instance.host.Peers)
        {
            input = NetworkUtils.GetNetworkData<PlayerInputData>(syncFromClients["input", peerID]);
            if (input != null)
                UpdateInput(clientPlayers[peerID], input);
        }
    }

    GameObject CreatePlayer(int peerID, ref int i)
    {
        DrifterType drifter = DrifterType.None;
        int playerIndex = 0;
        foreach (CharacterSelectState state in CharacterMenu.charSelStates)
        {
            if (state.PeerID == peerID)
            {
                drifter = state.PlayerType;
                playerIndex = state.PlayerIndex;
            }
        }
        GameObject obj = GameController.Instance.host.CreateNetworkObject(drifter.ToString().Replace("_", " "),
            spawnPoints[i % spawnPoints.Count].transform.position, Quaternion.identity);
        obj.GetComponent<Drifter>().SetColor(playerIndex);
        i++;
        obj.GetComponent<Drifter>().SetPeerId(peerID);
        players[peerID] = obj;
        return obj;
    }

    public static void UpdateInput(GameObject player, PlayerInputData input)
    {
        if (player == null)
            return;
        
        // if(input == null)
        //     input = player.GetComponent<Drifter>().prevInput[0];

        player.GetComponent<Drifter>().input = input;
        player.GetComponent<PlayerMovement>().UpdateInput();
        player.GetComponent<PlayerAttacks>().UpdateInput();

        //Input Buffering Stores the last 16 input states
        //Newest state is at index 0
        //Remove the oldest state each frame 
        for(int i = player.GetComponent<Drifter>().prevInput.Length - 2; i >=0; i--)
        {
            player.GetComponent<Drifter>().prevInput[i + 1] = (PlayerInputData)player.GetComponent<Drifter>().prevInput[i].Clone();
        }
        // add the newest state

        player.GetComponent<Drifter>().prevInput[0] = (PlayerInputData)input.Clone();
    }

    public static PlayerInputData GetInput(InputActionAsset keyBindings)
    {
        InputActionMap playerInputAction = keyBindings.FindActionMap("Player");
        PlayerInputData input = new PlayerInputData();
        // get player input

        input.Jump = playerInputAction.FindAction("Jump").ReadValue<float>() > 0 || playerInputAction.FindAction("Jump Alt").ReadValue<float>() > 0;
        input.Light = playerInputAction.FindAction("Light").ReadValue<float>() > 0;
        input.Special = playerInputAction.FindAction("Special").ReadValue<float>() > 0;
        input.Super = playerInputAction.FindAction("Grab").ReadValue<float>() > 0;
        input.Guard = playerInputAction.FindAction("Guard 1").ReadValue<float>() > 0;

        //input.Dash = playerInputAction.FindAction("Guard 2").ReadValue<float>() > 0;

        //TODO REIMPLEMENT GAMEPLAD CONTROLS
        //controller movement input
        // if (Input.GetJoystickNames().Length > 0)
        // {
        //     input.MoveX = Input.GetAxis("Horizontal");
        //     input.MoveY = Input.GetAxis("Vertical");
        // }

        // //keyboard movement input
        // else
        // {
        input.MoveX = 0;
            if (playerInputAction.FindAction("Left").ReadValue<float>() > 0)
            {
                input.MoveX--;
            }
            if (playerInputAction.FindAction("Right").ReadValue<float>() > 0) //|| Input.GetButtonDown("Horizontal"))
            {
                input.MoveX++;
            }
            input.MoveY = 0;
            if (playerInputAction.FindAction("Down").ReadValue<float>() > 0)
            {
                // down key does nothing
                input.MoveY--;
            }
            if (playerInputAction.FindAction("Up").ReadValue<float>() > 0)
            {
                input.MoveY++;
            }
        //}

        // if (playerInputAction.FindAction("Guard 1").triggered && playerInputAction.FindAction("Down").ReadValue<float>() > 0)
        // {
        //     input.MoveY--;
        // }



        return input;
    }
}


[Serializable]
public class PlayerInputData : INetworkData, ICloneable
{
    public string Type { get; set; }
    public float MoveX;
    public float MoveY;
    public bool Jump;
    public bool Light;
    public bool Special;
    public bool Super;
    public bool Guard;

    public object Clone()
    {
        return new PlayerInputData()
        {
            Type = Type,
            MoveX = MoveX,
            MoveY = MoveY,
            Jump = Jump,
            Light = Light,
            Special = Special,
            Super = Super,
            Guard = Guard
        };
    }

    public void CopyFrom(PlayerInputData data)
    {
        Type = data.Type;
        MoveX = data.MoveX;
        MoveY = data.MoveY;
        Jump = data.Jump;
        Light = data.Light;
        Special = data.Special;
        Super = data.Super;
        Guard = data.Guard;
    }
}