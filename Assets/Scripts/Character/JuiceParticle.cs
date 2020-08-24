using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuiceParticle : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    public int mode = 0;
    public Animator anim;
    public AudioSource audio;
    void Start()
    {
        StartCoroutine(Fade(duration));
        if(mode == 3 || mode == 4){
            audio.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("Particle",mode);
    }

    IEnumerator Fade(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
