using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    bool cleanupFlag;
    float cleanupTime;
    void FixedUpdate() {
        if (cleanupFlag && Time.time > cleanupTime)
            Destroy(gameObject);
    }

    public void Cleanup() {
        particles.Stop();
        cleanupFlag = true;

        cleanupTime = Time.time + particles.main.startLifetime.constantMax;
    }
}
