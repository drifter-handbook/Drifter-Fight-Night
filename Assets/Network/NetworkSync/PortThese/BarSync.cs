using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    public string Name = "MegurinBar";
    float time = 0f;


    public string Type { get; private set; }
    public int ID { get; set; } = 0;

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
    }

    public class BarData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float xScaleWind = 1f;
        public float yScaleWind = 1f;
        public float xScaleLightning = 1f;
        public float yScaleLightning = 1f;
        public float xScaleice = 1f;
        public float yScaleice = 1f;
    }

    public void Deserialize(INetworkEntityData data)
    {
        BarData projData = data as BarData;
        if (projData != null)
        {
            gameObject.transform.Find("WindSprite").transform.localScale = new Vector2(projData.xScaleWind,projData.yScaleWind);

            gameObject.transform.Find("LightningSprite").transform.localScale = new Vector2(projData.xScaleLightning,projData.yScaleLightning);

            gameObject.transform.Find("IceSprite").transform.localScale = new Vector2(projData.xScaleice,projData. yScaleice);

        }
    }

    public INetworkEntityData Serialize()
    {
        BarData data = new BarData()
        {
            name = gameObject.name,
            ID = ID,
            xScaleWind = gameObject.transform.Find("WindSprite").transform.localScale.x,
            yScaleWind = gameObject.transform.Find("WindSprite").transform.localScale.y,
            xScaleLightning = gameObject.transform.Find("LightningSprite").transform.localScale.x,
            yScaleLightning = gameObject.transform.Find("LightningSprite").transform.localScale.y,
            xScaleice = gameObject.transform.Find("IceSprite").transform.localScale.x,
            yScaleice = gameObject.transform.Find("IceSprite").transform.localScale.y,
        };
        return data;
    }
}
