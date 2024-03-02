using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterimageFollow : MonoBehaviour
{
    [SerializeField] private AfterimageSpawner spawner;

    [SerializeField] private float delay;
    [SerializeField] private float duration;
    [SerializeField] private int maxImages;
    private int images;
    private float nextImage;

    List<SpriteDurationPair> afterimages = new();

    private struct SpriteDurationPair {
        public SpriteRenderer sprite;
        public float duration;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (spawner.Ready() && images < maxImages && Time.time > nextImage) {
            var img = spawner.SpawnAfterimage();
            afterimages.Add(new SpriteDurationPair(){
                sprite = img.GetComponent<SpriteRenderer>(),
                duration = Time.time + duration
            });

            images++;
            nextImage = Time.time + delay;
        }

        for (int i = 0; i < afterimages.Count; i++) {

            var col = afterimages[i].sprite.color;
            col.a = (afterimages[i].duration - Time.time) / duration;
            afterimages[i].sprite.color = col;

            if (Time.time > afterimages[i].duration) {
                Destroy(afterimages[i].sprite.gameObject);
                afterimages.RemoveAt(i);
                i--;

                if (afterimages.Count == 0)
                    Destroy(gameObject);
            }
        }


    }
}
