using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedProjectileUtil : MonoBehaviour
{

	Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

   public void PlayAnimation(string state)
   {
   		if(!GameController.Instance.IsHost)return;
   		anim.Play(state);
   }
}
