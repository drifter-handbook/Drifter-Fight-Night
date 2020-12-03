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
}
