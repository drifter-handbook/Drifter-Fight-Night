using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummarySync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    float time = 0f;

    public string Type { get; private set; } = "Summary";
    public int ID { get; set; } = 0;

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
    }

    public class SummaryData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public bool pipCounter = false;
        public bool elementCounter = false;
    }

    public void Deserialize(INetworkEntityData data)
    {
        SummaryData summaryData = data as SummaryData;
        if (summaryData != null)
        {
            gameObject.transform.Find("megurinBars").gameObject.SetActive(summaryData.elementCounter);
            gameObject.transform.Find("Counter3Pip").gameObject.SetActive(summaryData.pipCounter);
        }
    }

    public INetworkEntityData Serialize()
    {
        SummaryData data = new SummaryData()
        {
            name = gameObject.name,
            ID = ID,
            pipCounter = gameObject.transform.Find("Counter3Pip").gameObject.activeSelf,
            elementCounter = gameObject.transform.Find("megurinBars").gameObject.activeSelf
        };
        return data;
    }
}
