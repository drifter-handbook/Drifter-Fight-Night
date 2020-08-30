using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    public string Name = "MegurinBar";
    float time = 0f;
    Vector3 oldScale;


    public string Type { get; private set; }
    public int ID { get; set; } = NetworkEntityList.NextID;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        Type = Name;
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
        transform.localScale = oldScale;
    }

    public class BarData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float xScale = 1f;
        public float yScale = 1f;
    }

    public void Deserialize(INetworkEntityData data)
    {
        BarData projData = data as BarData;
        if (projData != null)
        {
            oldScale = new Vector3(projData.xScale,projData.yScale,1);

        }
    }

    public INetworkEntityData Serialize()
    {
        BarData data = new BarData()
        {
            name = gameObject.name,
            ID = ID,
            xScale = gameObject.transform.localScale.x,
            yScale = gameObject.transform.localScale.y,
        };
        return data;
    }
}
