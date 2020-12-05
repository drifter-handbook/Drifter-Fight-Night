using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectGrab : MonoBehaviour
{
    public Drifter drifter;

    public string GrabState = "";

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!GameController.Instance.IsHost)return;
        if(col.gameObject.name == "Hurtboxes"
         && col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<Drifter>() != drifter
         && !col.GetComponent<HurtboxCollision>().parent.GetComponent<PlayerStatus>().HasStatusEffect(PlayerStatusEffect.INVULN)
         && col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<PlayerHurtboxHandler>().CanHit(gameObject.GetComponent<HitboxCollision>().AttackID))
        {
            if(GrabState != "")drifter.PlayAnimation(GrabState);
            else drifter.returnToIdle();
        }
    }
}
