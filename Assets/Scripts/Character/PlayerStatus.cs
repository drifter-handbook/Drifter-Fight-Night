using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatusEffect
{
    END_LAG, KNOCKBACK, INVULN, ARMOUR, HIT, PLANTED
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
        return HasStatusEffect(PlayerStatusEffect.END_LAG) || HasStatusEffect(PlayerStatusEffect.KNOCKBACK) || HasStatusEffect(PlayerStatusEffect.PLANTED);
    }
    public bool HasEnemyStunEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.KNOCKBACK) || HasStatusEffect(PlayerStatusEffect.PLANTED);
    }
    public bool IsEnemyStunEffect(PlayerStatusEffect ef){
        return (ef == PlayerStatusEffect.KNOCKBACK) || (ef == PlayerStatusEffect.PLANTED);
    }
    public bool HasGroundFriction()
    {
        return !HasStatusEffect(PlayerStatusEffect.KNOCKBACK);
    }
    public void ApplyStatusEffect(PlayerStatusEffect ef, float duration)
    {
        StartCoroutine(ApplyStatusEffectFor(ef, duration));
    }

    IEnumerator ApplyStatusEffectFor(PlayerStatusEffect ef, float duration)
    {
        UnityEngine.Debug.Log("APPLY: " +ef);
    	float delay = duration;
    	//Ignores hitstun if in superarmour or invuln
    	if((HasInulvernability() || HasArmour()) && ef ==  PlayerStatusEffect.KNOCKBACK){
    		yield break;
    	}

        //If youre planted, you get unplanted by a hit
        if(HasStatusEffect(PlayerStatusEffect.PLANTED) && IsEnemyStunEffect(ef))
        {
            UnityEngine.Debug.Log("PLANT RESET");
            statusEffects[PlayerStatusEffect.PLANTED] = 0f;
            UnityEngine.Debug.Log(statusEffects[PlayerStatusEffect.PLANTED]);
            UnityEngine.Debug.Log(ef);
            if(ef == PlayerStatusEffect.PLANTED)
            {
                UnityEngine.Debug.Log("OUTIE");
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
