using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinStorm : MonoBehaviour
{
    public PlayerAttacks attacks;
    // Start is called before the first frame update

    public void multihit(){
        foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.AttackID += 9;
                hitbox.AttackType = DrifterAttackType.W_Down;
                hitbox.Active = true;
            }
        
    }
}
