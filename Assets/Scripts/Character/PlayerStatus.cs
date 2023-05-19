using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatusEffect
{
	AMBERED,
    PLANTED,
    STUNNED,
    PARALYZED,
    GRABBED,
    CRINGE,
    DEAD,
    POISONED,
    BURNING,
    REVERSED,
    FLIGHT,
    INVULN,
    ARMOUR,
    EXPOSED,
    FEATHERWEIGHT,
    SLOWED,
    SPEEDUP,
    DAMAGEUP,
    DEFENSEUP,
    DEFENSEDOWN,
    HIT,
    END_LAG,
    KNOCKBACK,
    HITPAUSE,
    GUARDCRUSHED,
    STANCE,
    SLOWMOTION,
    HIDDEN,
    TUMBLE,
    KNOCKDOWN,
    FLATTEN,
}

public class PlayerStatusData
{
    public string name = "HIT";
    public int iconIndex = -1;
    public bool removeOnHit = true;
    public bool isStun = false;
    public bool isSelfInflicted = false;
    public int channel = 0;
    public GameObject statusBar = null;
    public int duration = 0;
    //public bool isMashable = false;

    public PlayerStatusData(string statusName, int icon = -1 ,bool remove = true,bool stun = false, bool self = false, int channel = 0) //, bool mashable = false)
    { 
        name = statusName;
        iconIndex = icon;
        removeOnHit = remove;
        isStun = stun;
        isSelfInflicted = self;
        this.channel = channel;

        //isMashable = mashable;
    }
}

public class PlayerStatus : MonoBehaviour
{

    public PlayerStatusData[] statusDataMap = new PlayerStatusData[]
    {
        
        new PlayerStatusData("AMBERED",icon: 3,stun: true)                                  ,
        new PlayerStatusData("PLANTED",icon: 3,stun:true)                                   ,
        new PlayerStatusData("STUNNED",icon:  3,stun: true)                                 ,
        new PlayerStatusData("PARALYZED",icon:  3,stun: true)                               ,
        new PlayerStatusData("GRABBED",icon:  3,stun: true)                                 ,
        new PlayerStatusData("CRINGE",icon: 3,stun: true)                                   ,
        new PlayerStatusData("DEAD",icon:  7,remove: false,stun: true)                      ,
        new PlayerStatusData("POISONED",icon:  0,remove: false)                             ,
        new PlayerStatusData("BURNING",icon: 1,remove: false)                               ,
        new PlayerStatusData("REVERSED",icon: 4)                                            ,
        new PlayerStatusData("FLIGHT",icon: 12, self: true)                                 ,
        new PlayerStatusData("INVULN",icon: 9, remove: false, self: true)                   ,
        new PlayerStatusData("ARMOUR",icon: 10, remove: false, self: true)                  ,
        new PlayerStatusData("EXPOSED",icon: 2,channel: 1)                                  ,
        new PlayerStatusData("FEATHERWEIGHT",icon: 2,remove: false, channel: 1)             ,
        new PlayerStatusData("SLOWED",icon: 11,remove: false,channel: 3)                    ,
        new PlayerStatusData("SPEEDUP",icon: 5,remove: false,self: true, channel: 3)        ,
        new PlayerStatusData("DAMAGEUP",icon: 6,remove: false,self: true, channel: 4)       ,
        new PlayerStatusData("DEFENSEUP",icon: 13,remove: false,self :true, channel: 5)     ,
        new PlayerStatusData("DEFENSEDOWN",icon: 8,remove: false, channel: 5)               ,
        new PlayerStatusData("HIT")                                                         ,
        new PlayerStatusData("END_LAG",stun: true, self: true)                              ,
        new PlayerStatusData("KNOCKBACK",remove: false, stun: true)                         ,
        new PlayerStatusData("HITPAUSE",stun: true, self:true)                              ,
        new PlayerStatusData("GUARDCRUSHED",icon: 14)                                       ,
        new PlayerStatusData("STANCE",remove: false,self: true)                             ,
        new PlayerStatusData("SLOWMOTION",icon: 16)                                         ,
        new PlayerStatusData("HIDDEN",remove: false)                                        ,
        new PlayerStatusData("TUMBLE")                                                      ,
        new PlayerStatusData("KNOCKDOWN", icon: 3,stun: true)                               ,
        new PlayerStatusData("FLATTEN")                                                     ,
    };

    


    Vector2 delayedVelocity;
    PlayerStatusEffect delayedEffect;
    int delayedEffectDuration;

    [NonSerialized]
    public Collider2D grabPoint = null;
    

    [NonSerialized]
    public PlayerCard card;
    [NonSerialized]
    public bool isInCombo = false;
    [SerializeField]
    private PlayerDamageNumbers damageDisplay;
    Drifter drifter;

    // Start is called before the first frame update
    void Start()
    {
        drifter = GetComponent<Drifter>();
        if(!GameController.Instance.IsTraining)ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,111);
    }
    
    // Update is called once per frame
    public void UpdateFrame()
    {
        if(GameController.Instance.IsPaused)
            return;

        if(grabPoint!=null && HasStatusEffect(PlayerStatusEffect.GRABBED) && grabPoint.enabled)
        {
            //drifter.movement.rb.position = grabPoint.bounds.center;
            drifter.transform.position = grabPoint.bounds.center;
        }

        else if(HasStatusEffect(PlayerStatusEffect.GRABBED) && (grabPoint== null || !grabPoint.enabled))
        {
            grabPoint=null;
            statusDataMap[(int)PlayerStatusEffect.GRABBED].duration = 0;
            drifter.movement.rb.velocity = delayedVelocity;
            delayedVelocity = Vector2.zero;
        }
        
            //Hitpause pauses all other statuses for its duration
            if(HasStatusEffect(PlayerStatusEffect.HITPAUSE) || HasStatusEffect(PlayerStatusEffect.GRABBED))
            {
                statusDataMap[(int)PlayerStatusEffect.HITPAUSE].duration--;
                if(!HasStatusEffect(PlayerStatusEffect.HITPAUSE) && !HasStatusEffect(PlayerStatusEffect.SLOWMOTION))
                {
                    if(delayedVelocity != Vector2.zero)drifter.movement.rb.velocity = delayedVelocity;
                    if(delayedEffect != PlayerStatusEffect.HIT)
                    {
                        ApplyStatusEffect(delayedEffect,delayedEffectDuration);
                        delayedEffect = PlayerStatusEffect.HIT;
                    }
                }
            }
            //Otherwise, tick down all active statuses
            else{
                for(int i = 0; i < statusDataMap.Length; i++)
                //for(int i = 0; i < statusDataMap.Length; i++)
                {
                    if(HasStatusEffect(i))
                    {
                        statusDataMap[i].duration--;

                        //Damage player if they are on fire
                        if(i == (int)PlayerStatusEffect.BURNING) drifter.DamageTaken += Time.fixedDeltaTime;
                        
                        //Re-apply the saved velocity if the player just lost cringe
                        
                        if(i == (int)PlayerStatusEffect.SLOWMOTION && HasEnemyStunEffect())drifter.movement.rb.velocity = delayedVelocity * .2f;


                        if((i == (int)PlayerStatusEffect.CRINGE && !HasStatusEffect(PlayerStatusEffect.CRINGE)) || (i == (int)PlayerStatusEffect.SLOWMOTION && !HasStatusEffect(PlayerStatusEffect.SLOWMOTION)))
                        {
                            drifter.movement.rb.velocity = delayedVelocity;
                            drifter.SetAnimationSpeed(1f);
                        }

                        if(i == (int)PlayerStatusEffect.FLATTEN)
                        {

                            //Wakeup if knocked off stage
                            if(!drifter.movement.grounded)
                                ApplyStatusEffect(PlayerStatusEffect.FLATTEN,0);

                            if(!HasStatusEffect(PlayerStatusEffect.FLATTEN))
                            {
                                ApplyStatusEffect(PlayerStatusEffect.KNOCKDOWN,8);
                                drifter.movement.hitstun = true;
                                drifter.knockedDown = false;
                                drifter.PlayAnimation("Jump_End");
                                ApplyStatusEffect(PlayerStatusEffect.INVULN,5);
                            }
                        }

                    }
                }
            }
            
        if(delayedVelocity != Vector2.zero && !(HasStatusEffect(PlayerStatusEffect.HITPAUSE) || HasStatusEffect(PlayerStatusEffect.CRINGE) || HasStatusEffect(PlayerStatusEffect.GRABBED) || HasStatusEffect(PlayerStatusEffect.SLOWMOTION))) delayedVelocity = Vector2.zero;

        if(isInCombo && !HasEnemyStunEffect()) isInCombo = false; 
        
    }

    //Returns true if the player has the specified status and its duration is not zero
    public bool HasStatusEffect(PlayerStatusEffect ef)
    {
        return HasStatusEffect((int)ef);
    }

    private bool HasStatusEffect(int ef)
    {
        return statusDataMap[ef].duration > 0;   
    }

    //Returns true if player is not actionable
    public bool HasStunEffect()
    {
        for(int i = 0; i < statusDataMap.Length; i++)
        {
            if(HasStatusEffect(i) && statusDataMap[i].isStun) return true;
        }
    	return false;
    }

    public Vector2 getDelayedVelocity()
    {
        return delayedVelocity;
    }

    public void setDelayedVelocity(Vector2 p_vel)
    {
        delayedVelocity = p_vel;
    }

    //Returns true if player is not actionable by an enemy's hand
    public bool HasEnemyStunEffect()
    {
        for(int i = 0; i < statusDataMap.Length; i++)
        {
            if(HasStatusEffect(i) && statusDataMap[i].isStun && !statusDataMap[i].isSelfInflicted) return true;
        }
        return false;        
    }

    public bool hasAdditionalStunEffect()
    {
        for(int i = 0; i < statusDataMap.Length; i++)
        {
            if(HasStatusEffect(i) && statusDataMap[i].isStun && !statusDataMap[i].isSelfInflicted && i != (int)PlayerStatusEffect.KNOCKBACK  && i != (int)PlayerStatusEffect.HITPAUSE) 
            {
                
                return true;
            }
        }
        return false;
    }
    public bool canbeKnockedDown()
    {
        if(hasAdditionalStunEffect())return false;
        return HasStatusEffect(PlayerStatusEffect.TUMBLE);        
    }

    public void saveXVelocity(float p_vel)
    {
        delayedVelocity = new Vector2(p_vel,delayedVelocity.y);
    }

    public void saveYVelocity(float p_vel)
    {
        delayedVelocity = new Vector2(delayedVelocity.x,p_vel);
    }

    //Player is not in hitstun
    public bool HasGroundFriction()
    {
        return !HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && !HasStatusEffect(PlayerStatusEffect.KNOCKDOWN); 
    }

    public void ApplyStatusEffect(PlayerStatusEffect ef, int duration)
    {
        ApplyStatusEffectFor(ef, duration);
    }

    public void ApplyDelayedStatusEffect(PlayerStatusEffect p_ef, int p_duration)
    {
        if(!HasStatusEffect(PlayerStatusEffect.HITPAUSE) && !HasStatusEffect(PlayerStatusEffect.SLOWMOTION))ApplyStatusEffectFor(p_ef, p_duration);
        else
        {
            delayedEffect = p_ef;
            delayedEffectDuration = p_duration;
        }
        
    }

    public void ApplyDamage(float damage, int hitstun) {

        damageDisplay.Increment(damage, isInCombo, hitstun);
    }

    //Clears all removable Status effects
    public void clearRemoveOnHitStatus()
    {
		for(int i = 0; i < statusDataMap.Length; i++)
        {
            if(statusDataMap[i].removeOnHit) 
                statusDataMap[i].duration = 0;
        
        }
        drifter.SetAnimationSpeed(1f);
        grabPoint = null;
    }


    //Clears all stun Status effects
    public void clearStunStatus()
    {
        for(int i = 0; i < statusDataMap.Length; i++)
        {
            if(statusDataMap[i].isStun)
                statusDataMap[i].duration = 0;

        }
        grabPoint = null;
    }

	//Clears ALL status effects    
    public void clearAllStatus()
    {
        for(int i = 0; i < statusDataMap.Length; i++)
            statusDataMap[i].duration = 0;

        grabPoint = null;
        drifter.SetAnimationSpeed(1f);
    }

    //Clears ALL status effects on a given status channel    
    public void clearStatusChannel(int channel)
    {
        for(int i = 0; i < statusDataMap.Length; i++)
        {
            if(statusDataMap[i].channel == channel)
                statusDataMap[i].duration = 0;


        }
    }

    public void clearVelocity()
    {
        delayedVelocity = Vector2.zero;
    }

    //Gets the remaining duration for a given stats effect
    public int remainingDuration(PlayerStatusEffect ef)
    {
    	return statusDataMap[(int)ef].duration;
    }

    //Reduces hitstun duration when restituted
    public void bounce()
    {
        if(HasStatusEffect(PlayerStatusEffect.KNOCKBACK)){
            statusDataMap[(int)PlayerStatusEffect.KNOCKBACK].duration = (int)(statusDataMap[(int)PlayerStatusEffect.KNOCKBACK].duration * .8f);
        }
    }

	//IDK fam. do we want to keep this?
    public int GetStatusToRender()
    {
    	//UnityEngine.Debug.Log("ASKED");
        if(HasStatusEffect(PlayerStatusEffect.AMBERED))return 1;
        if(HasStatusEffect(PlayerStatusEffect.PLANTED))return 2;
        if(HasStatusEffect(PlayerStatusEffect.PARALYZED))return 3;
        if(HasStatusEffect(PlayerStatusEffect.EXPOSED))return 4;
        if(HasStatusEffect(PlayerStatusEffect.FEATHERWEIGHT))return 5;
        if(HasStatusEffect(PlayerStatusEffect.REVERSED))return 6;
        if(HasStatusEffect(PlayerStatusEffect.SLOWED))return 7;
        if(HasStatusEffect(PlayerStatusEffect.INVULN))return 8;
        if(HasStatusEffect(PlayerStatusEffect.GRABBED))return 9;
        if(HasStatusEffect(PlayerStatusEffect.DEFENSEDOWN))return 10;
        return 0;
    }

    //initialize a status bar on the players summary card.
    GameObject addStatusBar(PlayerStatusEffect ef, int duration)
    {
    	if(card==null)return null;

    	return card.addStatusBar(ef,statusDataMap[(int)ef].iconIndex,duration,this);
    }


    //God this bullshit...
    void ApplyStatusEffectFor(PlayerStatusEffect ef, int duration)
    {

    	PlayerStatusData data = statusDataMap[(int)ef];

        // //If duration is 0, always clear the status
        if(duration <= 0 && HasStatusEffect(ef))
        {
            data.duration = 0;
            return;
        }

    	if(!HasStatusEffect(ef) && data.statusBar == null && !(data.iconIndex < 0))data.statusBar = addStatusBar(ef,duration);

    	//Ignores hitstun if in superarmour or invuln
        if(ef == PlayerStatusEffect.DEAD){
            clearAllStatus();
            data.duration = duration;
            return;
        }

        if(data.channel != 0)clearStatusChannel(data.channel);

        if(ef == PlayerStatusEffect.PARALYZED)drifter.movement.rb.velocity = new Vector2(0,15f);

    	if((HasStatusEffect(PlayerStatusEffect.INVULN) || HasStatusEffect(PlayerStatusEffect.ARMOUR)) && data.isStun && !data.isSelfInflicted) return;
    

        //Disallow unique stuns on already stunned opponents.
        //TODO See if this is necessary when plants are reintroduced
        if((data.isStun && !data.isSelfInflicted && HasStatusEffect(ef)) && ef != PlayerStatusEffect.KNOCKBACK || (HasStatusEffect(PlayerStatusEffect.PLANTED) && (ef == PlayerStatusEffect.GRABBED))){
            
            statusDataMap[(int)PlayerStatusEffect.KNOCKBACK].duration = 30;
            clearRemoveOnHitStatus();
            return;
        }
        //If youre planted or stunned, you get unplanted by a hit

        if((ef == PlayerStatusEffect.KNOCKBACK || data.isStun && !data.isSelfInflicted) && duration >0)clearRemoveOnHitStatus();        
        
        //save delayed velocity
        if((ef == PlayerStatusEffect.HITPAUSE || ef == PlayerStatusEffect.CRINGE || ef == PlayerStatusEffect.GRABBED || ef == PlayerStatusEffect.SLOWMOTION ||(ef == PlayerStatusEffect.KNOCKBACK &&  HasStatusEffect(PlayerStatusEffect.SLOWMOTION)))&& drifter.movement.rb.velocity != Vector2.zero) delayedVelocity = drifter.movement.rb.velocity;

        //Slow down animation speed in slowmo
        if(ef == PlayerStatusEffect.SLOWMOTION)
            drifter.SetAnimationSpeed(.4f);

        if(data.isStun && !data.isSelfInflicted) isInCombo = true;

    	data.duration = duration;

    }

    //Rollback
    //====================================

    //Takes a snapshot of the current frame to rollback to
    public StatusRollbackFrame SerializeFrame()
    {
        int[] statusList = new int[statusDataMap.Length];
        for(int i = 0; i < statusDataMap.Length;i++)
            statusList[i] = statusDataMap[i].duration;

        return new StatusRollbackFrame()
        {
            StatusList = statusList,
            DelayedVelocity = delayedVelocity,
            DelayedEffect = delayedEffect,
            DelayedEffectDuration = delayedEffectDuration,
            GrabPoint = grabPoint,
            

        };
    }

    //Rolls back the entity to a given frame state
    public  void DeserializeFrame(StatusRollbackFrame p_frame)
    {

        delayedVelocity = p_frame.DelayedVelocity;
        delayedEffect = (PlayerStatusEffect)p_frame.DelayedEffect;
        delayedEffectDuration = p_frame.DelayedEffectDuration;
        grabPoint = p_frame.GrabPoint;
        for(int i = 0; i < statusDataMap.Length;i++)
            statusDataMap[i].duration = p_frame.StatusList[i];

    }

}

public class StatusRollbackFrame: INetworkData
{
    public string Type { get; set; }

    public Vector2 DelayedVelocity;
    public PlayerStatusEffect DelayedEffect;
    public int DelayedEffectDuration;
    public Collider2D GrabPoint = null;

    public int[] StatusList;

}
