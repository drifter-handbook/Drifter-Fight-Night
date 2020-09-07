using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongArmSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    public string Name = "LongArmOfTheLaw";
    float time = 0f;
    Vector3 oldPos;
    Vector3 oldScale;
    Vector3 targetPos;
    float targetAngle;

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
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y,targetAngle);
    }

    public class ArmData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float xScale = 1f;
        public float yScale = 1f;
        public float angle = 0f;
        public bool destroy = false;
    }

    public void Deserialize(INetworkEntityData data)
    {
        ArmData projData = data as ArmData;
        if (projData != null)
        {
            if (!Active)
            {
                transform.position = new Vector3(projData.x, projData.y, projData.z);
            }
            Active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            oldPos = gameObject.transform.position;
            oldScale = new Vector3(projData.xScale,projData.yScale,1);
            targetPos = new Vector3(projData.x, projData.y, projData.z);
            targetAngle = projData.angle;
            gameObject.GetComponent<LongArm>().destroy = projData.destroy;
        }
    }

    public INetworkEntityData Serialize()
    {
        ArmData data = new ArmData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            xScale = gameObject.transform.localScale.x,
            yScale = gameObject.transform.localScale.y,
            angle = gameObject.transform.eulerAngles.z,
            destroy = gameObject.GetComponent<LongArm>().destroy
        };
        return data;
    }
}
