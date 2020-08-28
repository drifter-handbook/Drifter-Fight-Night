﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatusEffect
{
    AMBERED, PLANTED, STUNNED, EXPOSED, HIT, FEATHERWEIGHT, END_LAG, KNOCKBACK, INVULN, ARMOUR, REVERSED, SLOWED
}



public class PlayerStatus : MonoBehaviour
{
    float time = 0f;
    Dictionary<PlayerStatusEffect, float> statusEffects = new Dictionary<PlayerStatusEffect, float>();

    PlayerStatusEffect[] removeableEffects = {PlayerStatusEffect.STUNNED,PlayerStatusEffect.END_LAG,PlayerStatusEffect.REVERSED,PlayerStatusEffect.PLANTED,PlayerStatusEffect.EXPOSED};
    System.Array allEffects = PlayerStatusEffect.GetValues(typeof(PlayerStatusEffect));
    // Start is called before the first frame update
    void Start()
    {
    }
 
    // Update is called once per frame
    void Update()
    {
        if(time > .1f){
            time = 0f;
            foreach(PlayerStatusEffect ef in allEffects){
                if(HasStatusEffect(ef) &&statusEffects[ef] > 0)
                {
                    statusEffects[ef]--;
                }
                else{
                    statusEffects[ef] = 0;
                }
            }
        }
        time += Time.deltaTime;
        
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

    public bool HasRemovableEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.END_LAG) || HasStatusEffect(PlayerStatusEffect.PLANTED) || HasStatusEffect(PlayerStatusEffect.STUNNED)||  HasStatusEffect(PlayerStatusEffect.EXPOSED) || HasStatusEffect(PlayerStatusEffect.REVERSED);
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
        ApplyStatusEffectFor(ef, duration);
    }

    public void clearStatus()
    {
        foreach(PlayerStatusEffect ef in removeableEffects)
        {
         if(statusEffects.ContainsKey(ef))statusEffects[ef] = 0f;
        }
    }

    public int GetStatusToRender()
    {
        if(HasStatusEffect(PlayerStatusEffect.AMBERED))return 1;
        if(HasStatusEffect(PlayerStatusEffect.PLANTED))return 2;
        if(HasStatusEffect(PlayerStatusEffect.STUNNED))return 3;
        if(HasStatusEffect(PlayerStatusEffect.EXPOSED))return 4;
        if(HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))return 5;
        if(HasStatusEffect(PlayerStatusEffect.REVERSED))return 6;
        if(HasStatusEffect(PlayerStatusEffect.SLOWED))return 7;


        return 0;
    }

    void ApplyStatusEffectFor(PlayerStatusEffect ef, float duration)
    {
    	//Ignores hitstun if in superarmour or invuln
    	if((HasInulvernability() || HasArmour()) && IsEnemyStunEffect(ef)){
    		return;
    	}
        UnityEngine.Debug.Log("OUTER:" + ef);

        //If youre planted or stunned, you get unplanted by a hit
        if(IsEnemyStunEffect(ef) && HasRemovableEffect())
        {
            UnityEngine.Debug.Log("INNER:" +ef);
            clearStatus();
            if(ef == PlayerStatusEffect.KNOCKBACK){
                statusEffects[ef] = duration * 10f;
            }
            return;
        }

        if (!statusEffects.ContainsKey(ef))
        {
            statusEffects[ef] = 0f;
        }

        //Uses most rescently applied duration

        statusEffects[ef] = duration * 10f;

        // if(duration == 0f){
        // 	sequence = 0f;
        // 	statusEffects[ef] = 0f;
        // }
        // else if(statusEffects[ef] == (duration + sequence)){
        // 	sequence += .01f;
        // 	delay += sequence;
        // 	statusEffects[ef] = delay;

        // 	yield return new WaitForSeconds(duration);
        // 	if(statusEffects[ef] == delay){
        // 		sequence = 0f;
        // 		statusEffects[ef] = 0f;
        // 	}
        // }
        // else{
        // 	delay = duration+ sequence;
        // 	statusEffects[ef] = delay;
        // 	yield return new WaitForSeconds(duration);
        // 	if(statusEffects[ef] == delay){
        // 		sequence = 0f;
        // 		statusEffects[ef] = 0f;
        // 	}
        // }

    }
}
