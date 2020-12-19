using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedProjectileUtil : MonoBehaviour
{
  SyncAnimatorStateHost anim;
	//Animator anim;
  // Start is called before the first frame update
  void Start()
  {
    if(!GameController.Instance.IsHost)return;
    anim = GetComponent<SyncAnimatorStateHost>();
  }

  public void PlayAnimation(string state)
  {
   		if(!GameController.Instance.IsHost)return;
   		anim.SetState(state);
  }

  void OnTriggerEnter2D(Collider2D col)
  {

    if(!GameController.Instance.IsHost)return;
    if(col.gameObject.name == "Hurtboxes" && col.GetComponent<HurtboxCollision>() != this.gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponentInChildren<HurtboxCollision>()) PlayAnimation("OnHitState");
  }

}
