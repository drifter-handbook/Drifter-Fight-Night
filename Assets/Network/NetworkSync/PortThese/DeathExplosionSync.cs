using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathExplosionSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;

    public string Type { get; private set; } = "DeathExplosion";
    public int ID { get; set; } = 0;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Initialize(int playerIndex)
    {
        if (playerIndex == -2)
        {
            gameObject.GetComponent<SingleSound>().PlayAudio();
        }
        else if (playerIndex >= 0)
        {
            gameObject.GetComponent<MultiSound>().PlayAudio(playerIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active)
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

    public class DeathExplosionData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float angle = 0f;
    }

    public void Deserialize(INetworkEntityData data)
    {
        DeathExplosionData objData = data as DeathExplosionData;
        if (objData != null)
        {
            if (!Active)
            {
                transform.position = new Vector3(objData.x, objData.y, objData.z);
                transform.eulerAngles = new Vector3(0f, 0f, objData.angle);
            }
            Active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            oldPos = transform.position;
            targetPos = new Vector3(objData.x, objData.y, objData.z);
        }
    }

    public INetworkEntityData Serialize()
    {
        DeathExplosionData data = new DeathExplosionData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            angle = transform.eulerAngles.z
        };
        return data;
    }

    public void Cleanup()
    {
        Destroy(gameObject);
    }
}
