using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSync : MonoBehaviour, INetworkSync
{
    bool active = false;
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;

    public string Type { get; private set; } = "Main Camera";
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
    }

    public class ShakeData : INetworkEntityData
    {
        public string Type { get; set; }
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float zoom = 0f;
    }

    public void Deserialize(INetworkEntityData data)
    {
        ShakeData shakeData = data as ShakeData;
        if (shakeData != null)
        {
            if (!active)
            {
                transform.position = new Vector3(shakeData.x, shakeData.y, shakeData.z);
            }
            active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            oldPos = transform.position;
            targetPos = new Vector3(shakeData.x, shakeData.y, shakeData.z);
            GetComponent<Camera>().orthographicSize = shakeData.zoom;
        }
    }

    public INetworkEntityData Serialize()
    {
        return new ShakeData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            zoom = GetComponent<Camera>().orthographicSize
        };
    }
}
