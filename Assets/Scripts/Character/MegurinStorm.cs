using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinStorm : MonoBehaviour
{
    // Start is called before the first frame update
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
        yield return new WaitForSeconds(7f);
        Destroy(gameObject);
        yield break;
    }
}
