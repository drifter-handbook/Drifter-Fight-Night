using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterimageBurst : MonoBehaviour
{
    [SerializeField] private AfterimageSpawner spawner;
    [SerializeField] private int count;
    [SerializeField] private float speed;
    [SerializeField] private AnimationCurve motion;
    private List<GameObject> images = new();
    private float startTime;
    // Start is called before the first frame update
    void Start()
    {
        if (spawner.Ready()) 
            for (int i = 0; i < count; i++) {
                Color col = Color.HSVToRGB(1f / count * i, 1, 1);
                col.a = 0.5f;
                images.Add(spawner.SpawnAfterimage(transform));
                images[i].GetComponent<SpriteRenderer>().color = col;
            }

        startTime = Time.time;
    }

    void FixedUpdate() {
        for (int i = 0; i < count; i++) {
            float angle = 360 / count * i;
            Vector2 dif = Quaternion.Euler(0, 0, angle) * (speed * motion.Evaluate(Time.time - startTime) * Vector2.right);

            images[i].transform.localPosition += (Vector3)dif;
        }

        if (Time.time > startTime + motion.keys[2].time)
            Destroy(gameObject);
    }
}
