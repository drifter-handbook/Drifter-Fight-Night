// collection of data on a single attack
using System.Collections.Generic;
using UnityEngine;

public enum HitSpark
{
    NONE, POKE, BASH, PIERCE, GRAB ,GUARD_WEAK, GUARD_STRONG, SPIKE, MAGICWEAK, CRIT
}


[CreateAssetMenu(fileName = "SingleAttackData", menuName = "VirtuaDrifter/SingleAttackData", order = 70)]
public class SingleAttackData : ScriptableObject
{
    public float AttackDamage = 10.0f;
    public float Knockback = 10.0f;
    public float KnockbackScale = .5f;
    public float HitStun = -1f;
    public float EndLag = 0.1f;
    public bool isGrab = false;
    public float AngleOfImpact = 45f;
    public PlayerStatusEffect StatusEffect = PlayerStatusEffect.HIT;
    public float StatusDuration =.1f;
    public HitSpark HitVisual = HitSpark.POKE;

    public int GetHitSpark(){
    	switch(HitVisual){
    		case HitSpark.NONE:
    			return 0;
    		case HitSpark.POKE:
    			return 1;
    		case HitSpark.BASH:
    			return 2;
    		case HitSpark.PIERCE:
    			return 3;
    		case HitSpark.GRAB:
    			return 4;
    		case HitSpark.GUARD_STRONG:
    			return 5;	
    		case HitSpark.GUARD_WEAK:
    			return 6;
            case HitSpark.SPIKE:
                return 7;
            case HitSpark.MAGICWEAK:
                return 8;
            case HitSpark.CRIT:
                return 9;               
    		
    		default:
    			return 0;		
    	}

    }

}
