using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeanSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    public string Name = "Bean";
    float time = 0f;
    Vector3 oldPos;
    Vector3 oldScale;
    Vector3 targetPos;

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
        transform.position = Vector3.Lerp(oldPos, targetPos, t);
        transform.localScale = oldScale;

    }

    public class BeanData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float xScale = 1f;
        public float yScale = 1f;
        public bool Up = false;
        public bool Down = false;
        public bool Side = false;
        public bool Neutral = false;
        public bool Hide = false;

    }

    public void Deserialize(INetworkEntityData data)
    {
        BeanData beanData = data as BeanData;
        if (beanData != null)
        {
            if (!Active)
            {
                transform.position = new Vector3(beanData.x, beanData.y, beanData.z);
            }
            Active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            oldPos = transform.position;
            targetPos = new Vector3(beanData.x, beanData.y, beanData.z);
            oldScale = new Vector3(beanData.xScale,beanData.yScale,1);
            gameObject.GetComponent<BeanWrangler>().Up = beanData.Up;
            gameObject.GetComponent<BeanWrangler>().Down = beanData.Down;
            gameObject.GetComponent<BeanWrangler>().Side = beanData.Side;
            gameObject.GetComponent<BeanWrangler>().Neutral = beanData.Neutral;
            gameObject.GetComponent<BeanWrangler>().Hide = beanData.Hide;

        }
    }

    public INetworkEntityData Serialize()
    {
        BeanData data = new BeanData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            xScale = gameObject.transform.localScale.x,
            yScale = gameObject.transform.localScale.y,
            Up = gameObject.GetComponent<BeanWrangler>().Up,
            Down = gameObject.GetComponent<BeanWrangler>().Down,
            Side = gameObject.GetComponent<BeanWrangler>().Side,
            Neutral = gameObject.GetComponent<BeanWrangler>().Neutral,
            Hide = gameObject.GetComponent<BeanWrangler>().Hide,

        };
        return data;
    }
}
