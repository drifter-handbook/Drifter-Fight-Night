using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongArm : MonoBehaviour
{

    public bool destroy = false;
    // Start is called before the first frame update
    public float duration;
    Animator anim;
    void Start()
    {

        StartCoroutine(delete());
        anim = GetComponent<Animator>();
    }

    IEnumerator delete(){
        yield return new WaitForSeconds(duration/1.5f);
        destroy = true;
        anim.SetTrigger("Break");
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        if(destroy){
            anim.SetTrigger("Break");
        }
    }
}
