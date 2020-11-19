using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroSpearSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;

    public string Type { get; private set; } = "NeroSpear";
    public int ID { get; set; } = 0;

    void Awake()
    {

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

    public class SpearData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public bool active = true;
    }

    public void Deserialize(INetworkEntityData data)
    {
        SpearData projData = data as SpearData;
        if (projData != null)
        {
            if (!Active)
            {
                transform.position = new Vector3(projData.x, projData.y, projData.z);
            }
            Active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            GetComponent<Animator>().SetBool("Active", projData.active);
            oldPos = transform.position;
            targetPos = new Vector3(projData.x, projData.y, projData.z);
        }
    }

    public INetworkEntityData Serialize()
    {
        SpearData data = new SpearData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            active = GetComponent<NeroSpear>().Active
        };
        return data;
    }
}
