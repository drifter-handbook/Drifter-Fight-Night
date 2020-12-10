using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementParticleMode
{
    Hidden, SmokeTrail, Land, Jump, DoubleJump, KickOff, Mash, Restitution , DarkRestitution
}

public class JuiceParticle : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    public MovementParticleMode mode = MovementParticleMode.Hidden;
    public Animator anim;
    public AudioSource audioSource;
    void Start()
    {
        StartCoroutine(Fade(duration));
        if(mode == MovementParticleMode.Jump || mode == MovementParticleMode.DoubleJump)
        {
            audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("Particle", (int)mode);
    }

    IEnumerator Fade(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
