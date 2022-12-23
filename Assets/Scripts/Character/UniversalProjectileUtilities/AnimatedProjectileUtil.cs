using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedProjectileUtil : MonoBehaviour
{

  public bool playStateWhenGrounded = false;
  Animator anim;

	//Animator anim;
  // Start is called before the first frame update
  void Awake()
  {
    anim = GetComponent<Animator>();
  }

  public void PlayAnimation(string state)
  {
   		anim.Play(state);
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    if(playStateWhenGrounded && (col.gameObject.tag == "Ground" || col.gameObject.tag == "Platform")) PlayAnimation("OnGroundState");
  }
}
