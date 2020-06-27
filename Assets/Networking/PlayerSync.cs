using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    bool active;
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;

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

    public void SyncTo(SyncToClientPacket.PlayerData playerData)
    {
        active = true;
        // move from current position to final position in latency seconds
        time = 0f;
        GetComponentInChildren<SpriteRenderer>().flipX = playerData.facing;
        oldPos = transform.position;
        targetPos = new Vector3(playerData.x, playerData.y, playerData.z);
    }
}
