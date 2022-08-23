using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using HouraiTeahouse.Backroll;

public class NetworkPlayers : MonoBehaviour
{

    //GGPO
    //BackrollSession ggpo;
    //BackrollErrorCode result;
    //BackrollSessionCallbacks cb;

    NetworkSyncToHost syncFromClients;

    public List<GameObject> spawnPoints;

    public GameObject playerInputPrefab;

    //Dictionary<int, GameObject> clientPlayers = new Dictionary<int, GameObject>();

    [NonSerialized]
    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    public static NetworkPlayers Instance => GameObject.FindGameObjectWithTag("NetworkPlayers")?.GetComponent<NetworkPlayers>();

    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.CreatePrefab(GameController.Instance.selectedStage);

        //populate spawn points
        spawnPoints = new List<GameObject>();
        for(int i = 0; i <4; i++)
            spawnPoints.Add(GameObject.Find("SpawnPoint" + i));

        syncFromClients = GetComponent<NetworkSyncToHost>();

        // create players
        foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values)
            CreatePlayer(charSel.PeerID);


        /* fill in all callback functions */
        // cb.SaveGameStateCallback = 
        // cb.LoadGameStateCallback = 
        // cb.FreeBuffer = 
        // cb.AdvanceFrame = 

        /* Start a new session */

        //BackrollSessionConfig config = new BackrollSessionConfig();
        //result = Backroll.StartSession(); 


    }

    // Update is called once per frame
    void FixedUpdate()
    {

        PlayerInputData input;

        foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values)
        {
            //Link inputs to peer ids
            input = NetworkUtils.GetNetworkData<PlayerInputData>(syncFromClients["input", charSel.PeerID]);
            if (input != null)
                UpdateInput(players[charSel.PeerID], input);

            else if(GameController.Instance.controls.ContainsKey(charSel.PeerID))
                UpdateInput(players[charSel.PeerID], GetInput(GameController.Instance.controls[charSel.PeerID]));
                
            else
                UpdateInput(players[charSel.PeerID]);

        }
        
        Physics2D.Simulate(1f/60f);
    }

    GameObject CreatePlayer(int peerID)
    {
        DrifterType drifter = DrifterType.None;
        foreach (CharacterSelectState state in CharacterMenu.charSelStates.Values)
        {
            if (state.PeerID == peerID)
                drifter = state.PlayerType;
        }

        //Same here
        GameObject obj = GameController.Instance.CreatePrefab(drifter.ToString().Replace("_", " "),
            spawnPoints[(peerID +1) % spawnPoints.Count].transform.position, Quaternion.identity);
        obj.GetComponent<Drifter>().SetColor((peerID +1));

        if(GameController.Instance.controls.ContainsKey(peerID))obj.GetComponent<Drifter>().playerInputController = GameController.Instance.controls[peerID];
        obj.GetComponent<Drifter>().SetPeerId(peerID);
        obj.GetComponent<PlayerMovement>().setFacing((peerID+1 % 2) * 2 - 1);
        players[peerID] = obj;
        return obj;
    }

    public static void UpdateInput(GameObject player, PlayerInputData input)
    {
        if (player == null)
            return;

        Drifter playerDrifter = player.GetComponent<Drifter>();

        for (int i = player.GetComponent<Drifter>().input.Length - 2; i >= 0; i--)
        {
            playerDrifter.input[i + 1] = (PlayerInputData)playerDrifter.input[i].Clone();
        }

        playerDrifter.input[0] = input;
        playerDrifter.UpdateFrame();

        playerDrifter.SerializeFrame();

    }

    public static void UpdateInput(GameObject player)
    {
        if(player == null)return;
        player.GetComponent<Drifter>().UpdateFrame();
    }

    public static PlayerInputData GetInput(PlayerInput playerInput)
    {
        InputActionMap playerInputAction = playerInput.currentActionMap;
        PlayerInputData input = new PlayerInputData();

        // get player input
        input.Jump = playerInputAction.FindAction("Jump").ReadValue<float>() > 0 || playerInputAction.FindAction("Jump Alt").ReadValue<float>() > 0;
        input.Light = playerInputAction.FindAction("Light").ReadValue<float>() > 0;
        input.Special = playerInputAction.FindAction("Special").ReadValue<float>() > 0;
        input.Super = playerInputAction.FindAction("Super").ReadValue<float>() > 0;
        input.Guard = playerInputAction.FindAction("Guard 1").ReadValue<float>() > 0;
        input.MoveX = playerInputAction.FindAction("Horizontal").ReadValue<float>();
        input.MoveY = playerInputAction.FindAction("Vertical").ReadValue<float>();
        input.Grab = playerInputAction.FindAction("Grab").ReadValue<float>() > 0;

        input.Pause = playerInputAction.FindAction("Start").ReadValue<float>()>0;

        input.Menu = playerInputAction.FindAction("Menu").ReadValue<float>()>0;

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
    public bool Pause;
    public bool Grab;
    public bool Menu;

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
            Guard = Guard,
            Pause = Pause,
            Grab = Grab,
            Menu = Menu,
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
        Pause = data.Pause;
        Grab = data.Grab;
        Menu = data.Menu;
    }
}
