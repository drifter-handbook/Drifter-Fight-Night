using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatusEffect
{
    AMBERED, PLANTED, STUNNED, EXPOSED, HIT, FEATHERWEIGHT, END_LAG, KNOCKBACK, INVULN, ARMOUR, REVERSED, SLOWED, DEAD
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
        return HasStatusEffect(PlayerStatusEffect.END_LAG) || HasStatusEffect(PlayerStatusEffect.KNOCKBACK) || HasStatusEffect(PlayerStatusEffect.PLANTED) || HasStatusEffect(PlayerStatusEffect.STUNNED)|| HasStatusEffect(PlayerStatusEffect.AMBERED) || HasStatusEffect(PlayerStatusEffect.DEAD);
    }

    public bool HasRemovableEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.PLANTED) || HasStatusEffect(PlayerStatusEffect.STUNNED)||  HasStatusEffect(PlayerStatusEffect.EXPOSED) || HasStatusEffect(PlayerStatusEffect.REVERSED)  || HasStatusEffect(PlayerStatusEffect.END_LAG);
    }
    public bool HasEnemyStunEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.KNOCKBACK) || HasStatusEffect(PlayerStatusEffect.PLANTED)|| HasStatusEffect(PlayerStatusEffect.STUNNED) || HasStatusEffect(PlayerStatusEffect.AMBERED) || HasStatusEffect(PlayerStatusEffect.DEAD);
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

    public Dictionary<PlayerStatusEffect,float> getStatusState(){
        return statusEffects;
    }
    public void setStatusState(Dictionary<PlayerStatusEffect,float> statusEffects){
        this.statusEffects = statusEffects;
    }

    public void bounce()
    {
        if(HasStatusEffect(PlayerStatusEffect.KNOCKBACK)){
            statusEffects[PlayerStatusEffect.KNOCKBACK] *= .8f;
        }
    }

    public void mashOut(){
        if(HasStatusEffect(PlayerStatusEffect.AMBERED))statusEffects[PlayerStatusEffect.AMBERED]--;
        if(HasStatusEffect(PlayerStatusEffect.PLANTED))statusEffects[PlayerStatusEffect.PLANTED]--;
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
        if(HasStatusEffect(PlayerStatusEffect.DEAD)){
            statusEffects[ef] = duration * 10f;
            return;
        }

    	if((HasInulvernability() || HasArmour()) && IsEnemyStunEffect(ef)){
    		return;
    	}

        //If youre planted or stunned, you get unplanted by a hit
        if(IsEnemyStunEffect(ef) && HasRemovableEffect())
        {
            clearStatus();
            if(ef == PlayerStatusEffect.KNOCKBACK){
                statusEffects[ef] = duration * 10f;
            }
            else if( ef == PlayerStatusEffect.PLANTED){
                 statusEffects[PlayerStatusEffect.KNOCKBACK] = 4f;
            }
            return;
        }

        if (!statusEffects.ContainsKey(ef))
        {
            statusEffects[ef] = 0f;
        }
        
        statusEffects[ef] = duration * 10f;

    }
}
