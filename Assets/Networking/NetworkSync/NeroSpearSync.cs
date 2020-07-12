using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroSpearSync : MonoBehaviour, INetworkSync
{
    bool active;
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;

    public string Type { get; private set; } = "NeroSpear";
    public int ID { get; set; } = NetworkEntityList.NextID;

    SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
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

    public class SpearData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float alpha = 0f;
    }

    public void Deserialize(INetworkEntityData data)
    {
        SpearData projData = data as SpearData;
        if (projData != null)
        {
            if (!active)
            {
                transform.position = new Vector3(projData.x, projData.y, projData.z);
            }
            active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, projData.alpha);
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
            alpha = sr.color.a
        };
        return data;
    }
}
