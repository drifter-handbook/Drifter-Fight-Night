﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultihitZoneProjectile : MonoBehaviour
{
    // Start is called before the first frame update

    public void multihit(){
        foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.AttackID += 9;
                hitbox.AttackType = DrifterAttackType.W_Down;
                hitbox.isActive = true;
            }
        
    }
}