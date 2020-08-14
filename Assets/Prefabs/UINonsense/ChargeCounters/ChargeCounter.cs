using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeCounter : MonoBehaviour
{
    
    public int charge;
    public Animator anim;

    void Start()
    {
    }

    void Update(){
        anim.SetInteger("Charge",charge);
    }

    public void setCharge(int Charge){
        charge = Charge;
    }

    IEnumerator Hide()
    {
        yield return new WaitForSeconds(2.5f);
        //anim.SetBool("Show",false);
        yield break;
    }


}
