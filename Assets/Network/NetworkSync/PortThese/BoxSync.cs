using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSync : MonoBehaviour, INetworkSync
{
    bool active = false;
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    float oldAngle;
    Vector3 targetPos;
    float targetAngle;

    public string Type { get; private set; } = "Box";
    public int ID { get; set; } = 0;

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
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y,
            Mathf.Lerp(oldAngle, targetAngle, t));
    }

    public class BoxData : INetworkEntityData
    {
        public string Type { get; set; }
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float angle = 0f;
    }

    public void Deserialize(INetworkEntityData data)
    {
        BoxData boxData = data as BoxData;
        if (boxData != null)
        {
            if (!active)
            {
                transform.position = new Vector3(boxData.x, boxData.y, boxData.z);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, boxData.angle);
            }
            active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            oldPos = transform.position;
            oldAngle = transform.eulerAngles.z;
            targetPos = new Vector3(boxData.x, boxData.y, boxData.z);
            targetAngle = oldAngle + Mathf.DeltaAngle(oldAngle, boxData.angle);
        }
    }

    public INetworkEntityData Serialize()
    {
        return new BoxData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            angle = transform.eulerAngles.z
        };
    }
}
