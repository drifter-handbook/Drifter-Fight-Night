using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSync : MonoBehaviour
{
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    float oldAngle;
    Vector3 targetPos;
    float targetAngle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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

    public void SyncTo(SyncToClientPacket.ObjectData objData)
    {
        // move from current position to final position in latency seconds
        time = 0f;
        oldPos = transform.position;
        oldAngle = transform.eulerAngles.z;
        targetPos = new Vector3(objData.x, objData.y, objData.z);
        targetAngle = objData.angle;
    }
}
