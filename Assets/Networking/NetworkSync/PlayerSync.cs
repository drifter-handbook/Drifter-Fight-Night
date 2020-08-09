using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncColor
{
    public float r;
    public float g;
    public float b;
    public float a;

    public SyncColor(Color c)
    {
        r = c.r;
        g = c.g;
        b = c.b;
        a = c.a;
    }

    public Color ToColor()
    {
        return new Color(r, g, b, a);
    }
}

public class PlayerSync : MonoBehaviour, INetworkSync
{
    bool active;
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;

    public string NetworkSyncType = "";

    public string Type { get { return NetworkSyncType; } }
    public int ID { get; set; } = NetworkEntityList.NextID;

    NetworkEntityList Entities;

    void Awake()
    {
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
        {
            return;
        }
        // lerp
        time = Mathf.MoveTowards(time, latency, Time.deltaTime);
        float t = 0;
        if (latency > 0)
        {
            t = time / latency;
        }
        transform.position = Vector3.Lerp(oldPos, targetPos, t);
    }

    public class PlayerData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public int facing = 1;
        public int stocks = 0;
        public float damageTaken = 0f;
        public PlayerAnimatorState animatorState = new PlayerAnimatorState();
        public SyncColor color;
    }

    public void Deserialize(INetworkEntityData data)
    {
        PlayerData playerData = data as PlayerData;
        if (playerData != null)
        {
            if (!active)
            {
                transform.position = new Vector3(playerData.x, playerData.y, playerData.z);
                gameObject.GetComponentInChildren<SpriteRenderer>().color = playerData.color.ToColor();
            }
            active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            gameObject.transform.localScale = new Vector3(
                playerData.facing * Mathf.Abs(gameObject.transform.localScale.x),
                gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            oldPos = transform.position;
            targetPos = new Vector3(playerData.x, playerData.y, playerData.z);
            GetComponent<Drifter>().SyncAnimatorState(playerData.animatorState);
            gameObject.GetComponent<Drifter>().Stocks = playerData.stocks;
            gameObject.GetComponent<Drifter>().DamageTaken = playerData.damageTaken;
            int index = GameController.Instance.LocalPlayer.PlayerIndex;
            if (index >= 0)
            {
                gameObject.GetComponent<Drifter>().SetColor(CharacterMenu.ColorFromEnum[(PlayerColor)index]);
            }
        }
    }

    public INetworkEntityData Serialize()
    {
        PlayerData data = new PlayerData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            facing = (int)Mathf.Sign(gameObject.transform.localScale.x),
            animatorState = (PlayerAnimatorState)gameObject.GetComponent<Drifter>().animatorState.Clone(),
            stocks = gameObject.GetComponent<Drifter>().Stocks,
            damageTaken = gameObject.GetComponent<Drifter>().DamageTaken,
            color = new SyncColor(gameObject.GetComponentInChildren<SpriteRenderer>().color)
        };
        GetComponent<Drifter>().ResetAnimatorTriggers();
        return data;
    }
}
