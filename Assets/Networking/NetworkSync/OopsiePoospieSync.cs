using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OopsiePoospieSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;
    bool oldEmp;

    public string Type { get; private set; } = "Amber";
    public int ID { get; set; } = NetworkEntityList.NextID;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
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
        GetComponent<OopsiePoopsie>().empowered = oldEmp;
        transform.position = Vector3.Lerp(oldPos, targetPos, t);
    }

    public class AmberData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float xScale = 1f;
        public float yScale = 1f;
        public bool empowered = false;
    }

    public void Deserialize(INetworkEntityData data)
    {
        AmberData fireballData = data as AmberData;
        if (fireballData != null)
        {
            if (!Active)
            {
                transform.position = new Vector3(fireballData.x, fireballData.y, fireballData.z);
            }
            Active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            oldEmp =  fireballData.empowered;
            oldPos = transform.position;
            targetPos = new Vector3(fireballData.x, fireballData.y, fireballData.z);
            transform.localScale =  new Vector3(fireballData.xScale,fireballData.yScale,1);
        }
    }

    public INetworkEntityData Serialize()
    {
        AmberData data = new AmberData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            empowered = GetComponent<OopsiePoopsie>().empowered,
            xScale = gameObject.transform.localScale.x,
            yScale = gameObject.transform.localScale.y,
        };
        return data;
    }
}
