using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DurationProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    void Start()
    {

        StartCoroutine(delete());
        
    }

    IEnumerator delete(){
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
        yield break;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
