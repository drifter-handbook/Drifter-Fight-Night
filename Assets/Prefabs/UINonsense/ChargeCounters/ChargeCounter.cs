using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeCounter : MonoBehaviour
{
    
    public int charge;
    public Animator anim;

    void Start()
    {
        //Deleted if the character doesnt use it
        if(charge == -1){
            Destroy(gameObject);
        }
    }

    void update(){
        if(charge != anim.GetInteger("Charge")){
            anim.SetInteger("Charge",charge);
            anim.SetBool("Show",true);
            StartCoroutine(Hide());
        }
        

    }

    IEnumerator Hide()
    {
        yield return new WaitForSeconds(2.5f);
        anim.SetBool("Show",false);
        yield break;
    }


}
