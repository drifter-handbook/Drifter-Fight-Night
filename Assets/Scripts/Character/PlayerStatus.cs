using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatusEffect
{
    AMBERED, PLANTED, STUNNED, EXPOSED, HIT, FEATHERWEIGHT, END_LAG, KNOCKBACK, INVULN, ARMOUR, REVERSED, SLOWED, DEAD, HITPAUSE, PARALYZED, GRABBED, HEXED
}



public class PlayerStatus : MonoBehaviour
{
    float time = 0f;
    Dictionary<PlayerStatusEffect, float> statusEffects = new Dictionary<PlayerStatusEffect, float>();

    PlayerStatusEffect[] removeableEffects = {PlayerStatusEffect.STUNNED,PlayerStatusEffect.END_LAG,PlayerStatusEffect.REVERSED,PlayerStatusEffect.PLANTED,PlayerStatusEffect.EXPOSED,PlayerStatusEffect.PARALYZED,PlayerStatusEffect.GRABBED};
    System.Array allEffects = PlayerStatusEffect.GetValues(typeof(PlayerStatusEffect));
    Rigidbody2D rb;
    Vector2 delayedVelocity;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
 
    // Update is called once per frame
    void Update()
    {
        if(time > .1f){
            time = 0f;
            //Hitpause pauses all other statuses for its duration
            if(HasStatusEffect(PlayerStatusEffect.HITPAUSE))
            {
                 statusEffects[PlayerStatusEffect.HITPAUSE]--;
                 if(!HasStatusEffect(PlayerStatusEffect.HITPAUSE))rb.velocity = delayedVelocity;
            }
            else{
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
        return HasStatusEffect(PlayerStatusEffect.END_LAG) || HasStatusEffect(PlayerStatusEffect.HITPAUSE) || HasStatusEffect(PlayerStatusEffect.GRABBED) || HasStatusEffect(PlayerStatusEffect.KNOCKBACK) || HasStatusEffect(PlayerStatusEffect.PLANTED) || HasStatusEffect(PlayerStatusEffect.PARALYZED) || HasStatusEffect(PlayerStatusEffect.STUNNED)|| HasStatusEffect(PlayerStatusEffect.AMBERED) || HasStatusEffect(PlayerStatusEffect.DEAD);
    }

    public bool HasRemovableEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.PLANTED) || HasStatusEffect(PlayerStatusEffect.STUNNED)|| HasStatusEffect(PlayerStatusEffect.GRABBED)||  HasStatusEffect(PlayerStatusEffect.EXPOSED) || HasStatusEffect(PlayerStatusEffect.REVERSED) || HasStatusEffect(PlayerStatusEffect.PARALYZED) || HasStatusEffect(PlayerStatusEffect.END_LAG);
    }
    public bool HasEnemyStunEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.KNOCKBACK) || HasStatusEffect(PlayerStatusEffect.PLANTED) || HasStatusEffect(PlayerStatusEffect.GRABBED) || HasStatusEffect(PlayerStatusEffect.STUNNED) || HasStatusEffect(PlayerStatusEffect.AMBERED) || HasStatusEffect(PlayerStatusEffect.PARALYZED) || HasStatusEffect(PlayerStatusEffect.DEAD);
    }
    public bool IsEnemyStunEffect(PlayerStatusEffect ef){
        return (ef == PlayerStatusEffect.KNOCKBACK) || (ef == PlayerStatusEffect.PLANTED) || (ef == PlayerStatusEffect.AMBERED) || (ef == PlayerStatusEffect.STUNNED || (ef == PlayerStatusEffect.PARALYZED) || (ef == PlayerStatusEffect.GRABBED));
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

    public void clearAllStatus()
    {
        foreach(PlayerStatusEffect ef in allEffects)
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
        //Adjust these numbers to make it easier or harder to mash out
        if(HasStatusEffect(PlayerStatusEffect.AMBERED))statusEffects[PlayerStatusEffect.AMBERED]-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.PLANTED))statusEffects[PlayerStatusEffect.PLANTED]-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.PARALYZED))statusEffects[PlayerStatusEffect.PARALYZED]-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.PARALYZED))statusEffects[PlayerStatusEffect.GRABBED]-=.4f;
    }

    public int GetStatusToRender()
    {
        if(HasStatusEffect(PlayerStatusEffect.AMBERED))return 1;
        if(HasStatusEffect(PlayerStatusEffect.PLANTED))return 2;
        if(HasStatusEffect(PlayerStatusEffect.PARALYZED))return 3;
        if(HasStatusEffect(PlayerStatusEffect.EXPOSED))return 4;
        if(HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))return 5;
        if(HasStatusEffect(PlayerStatusEffect.REVERSED))return 6;
        if(HasStatusEffect(PlayerStatusEffect.SLOWED))return 7;
        if(HasStatusEffect(PlayerStatusEffect.INVULN))return 8;
        if(HasStatusEffect(PlayerStatusEffect.GRABBED))return 9;
        if(HasStatusEffect(PlayerStatusEffect.HEXED))return 10;
        return 0;
    }

    void ApplyStatusEffectFor(PlayerStatusEffect ef, float duration)
    {
    	//Ignores hitstun if in superarmour or invuln
        if(ef == PlayerStatusEffect.DEAD){
            foreach(PlayerStatusEffect effect in allEffects){
                    if(HasStatusEffect(effect))statusEffects[effect] = 0;                   
            }
            statusEffects[ef] = duration * 10f;
            return;
        }

        if(ef == PlayerStatusEffect.PARALYZED){
            rb.velocity = Vector2.zero;
        }

    	if((HasInulvernability() || HasArmour()) && IsEnemyStunEffect(ef)){
    		return;
    	}

        if((IsEnemyStunEffect(ef) && HasStatusEffect(ef))|| (HasStatusEffect(PlayerStatusEffect.PLANTED) && (ef == PlayerStatusEffect.GRABBED))){
            statusEffects[PlayerStatusEffect.KNOCKBACK] = 5.5f;
            clearStatus();
            return;
        }
        //If youre planted or stunned, you get unplanted by a hit
        if((ef == PlayerStatusEffect.KNOCKBACK || IsEnemyStunEffect(ef))&& HasRemovableEffect())clearStatus();        

        if (!statusEffects.ContainsKey(ef))
        {
            statusEffects[ef] = 0f;
        }
        
        if(ef == PlayerStatusEffect.HITPAUSE){
            //save delayed velocity
            delayedVelocity = rb.velocity;
        }


        statusEffects[ef] = duration * 10f;

    }
}
