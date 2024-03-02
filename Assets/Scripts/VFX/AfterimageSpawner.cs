using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterimageSpawner : MonoBehaviour
{
    private SpriteRenderer afterimageSource;
    [SerializeField] private GameObject afterimagePrefab;

    public GameObject SpawnAfterimage(Transform parent = null) {
        GameObject fx = Instantiate(afterimagePrefab, transform.position - Vector3.back, Quaternion.identity, parent);
        if (parent != null)
            fx.transform.localScale = Vector3.one;
        SpriteRenderer sprite = fx.GetComponent<SpriteRenderer>();
        sprite.sprite = afterimageSource.sprite;

        if (parent == null)
            sprite.flipX = transform.lossyScale.x < 0;

        return fx;
    }

    public void Init(SpriteRenderer source) {
        afterimageSource = source;
    }

    public bool Ready() {
        return afterimageSource != null;
    }
}
