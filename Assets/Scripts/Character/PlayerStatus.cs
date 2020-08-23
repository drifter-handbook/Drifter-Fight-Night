using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatusEffect
{
    AMBERED, PLANTED, STUNNED, EXPOSED, HIT, FEATHERWEIGHT, END_LAG, KNOCKBACK, INVULN, ARMOUR, REVERSED
}



public class PlayerStatus : MonoBehaviour
{
    Dictionary<PlayerStatusEffect, float> statusEffects = new Dictionary<PlayerStatusEffect, float>();
	float sequence = 0f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    public bool HasInulvernability()
    {
        return HasStatusEffect(PlayerStatusEffect.INVULN);
    }

    public bool HasHit()
    {
        return HasStatusEffect(PlayerStatusEffect.HIT);
    }

    public bool HasArmour()
    {
        return HasStatusEffect(PlayerStatusEffect.ARMOUR);
    }
    public bool HasStatusEffect(PlayerStatusEffect ef)
    {
        return statusEffects.ContainsKey(ef) && statusEffects[ef] > 0;
    }
    public bool HasStunEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.END_LAG) || HasStatusEffect(PlayerStatusEffect.KNOCKBACK) || HasStatusEffect(PlayerStatusEffect.PLANTED) || HasStatusEffect(PlayerStatusEffect.STUNNED)|| HasStatusEffect(PlayerStatusEffect.AMBERED);
    }
    public bool HasEnemyStunEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.KNOCKBACK) || HasStatusEffect(PlayerStatusEffect.PLANTED)|| HasStatusEffect(PlayerStatusEffect.STUNNED) || HasStatusEffect(PlayerStatusEffect.AMBERED);
    }
    public bool IsEnemyStunEffect(PlayerStatusEffect ef){
        return (ef == PlayerStatusEffect.KNOCKBACK) || (ef == PlayerStatusEffect.PLANTED) || (ef == PlayerStatusEffect.AMBERED) || (ef == PlayerStatusEffect.STUNNED);
    }
    public bool HasGroundFriction()
    {
        return !HasStatusEffect(PlayerStatusEffect.KNOCKBACK);
    }
    public void ApplyStatusEffect(PlayerStatusEffect ef, float duration)
    {
        StartCoroutine(ApplyStatusEffectFor(ef, duration));
    }

    public int GetStatusToRender()
    {
        // int powerTwoCode = 0;
        // if(HasStatusEffect(PlayerStatusEffect.AMBERED))powerTwoCode+=1;
        // if(HasStatusEffect(PlayerStatusEffect.PLANTED))powerTwoCode+=2;
        // if(HasStatusEffect(PlayerStatusEffect.STUNNED))powerTwoCode+=4;
        // if(HasStatusEffect(PlayerStatusEffect.EXPOSED))powerTwoCode+=8;
        // if(HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))powerTwoCode+=16;
        // return powerTwoCode;

        if(HasStatusEffect(PlayerStatusEffect.AMBERED))return 1;
        if(HasStatusEffect(PlayerStatusEffect.PLANTED))return 2;
        if(HasStatusEffect(PlayerStatusEffect.STUNNED))return 3;
        if(HasStatusEffect(PlayerStatusEffect.EXPOSED))return 4;
        if(HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))return 5;
        if(HasStatusEffect(PlayerStatusEffect.REVERSED))return 6;

        return 0;
    }

    IEnumerator ApplyStatusEffectFor(PlayerStatusEffect ef, float duration)
    {
    	float delay = duration;
    	//Ignores hitstun if in superarmour or invuln
    	if((HasInulvernability() || HasArmour()) && IsEnemyStunEffect(ef)){
    		yield break;
    	}

        //If youre planted, you get unplanted by a hit
        if(HasStatusEffect(PlayerStatusEffect.PLANTED) && IsEnemyStunEffect(ef) && duration != 0f)
        {
            statusEffects[PlayerStatusEffect.PLANTED] = 0f;
            statusEffects[PlayerStatusEffect.STUNNED] = 0f;
            if(ef == PlayerStatusEffect.PLANTED)
            {
                yield break;
            }
        
        }

        if (!statusEffects.ContainsKey(ef))
        {
            statusEffects[ef] = 0f;
        }

        if(ef ==  PlayerStatusEffect.KNOCKBACK && statusEffects.ContainsKey(PlayerStatusEffect.END_LAG)){
            statusEffects[PlayerStatusEffect.END_LAG] = 0f;
        }

        //Uses most rescently applied duration

        if(duration == 0f){
        	sequence = 0f;
        	statusEffects[ef] = 0f;
        }
        else if(statusEffects[ef] == (duration + sequence)){
        	sequence += .01f;
        	delay += sequence;
        	statusEffects[ef] = delay;

        	yield return new WaitForSeconds(duration);
        	if(statusEffects[ef] == delay){
        		sequence = 0f;
        		statusEffects[ef] = 0f;
        	}
        }
        else{
        	delay = duration+ sequence;
        	statusEffects[ef] = delay;
        	yield return new WaitForSeconds(duration);
        	if(statusEffects[ef] == delay){
        		sequence = 0f;
        		statusEffects[ef] = 0f;
        	} 
        }
        
    }
}
