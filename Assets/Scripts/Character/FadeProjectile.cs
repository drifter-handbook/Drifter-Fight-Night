using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    void Start()
    {
        StartCoroutine(Fade());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
        yield break;
    }
}
