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
	DEAD ,    
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
    GUARDBROKEN, 
}

class PlayerStatusData
{
	public static Dictionary<PlayerStatusEffect,PlayerStatusData> statusDataMap = new Dictionary<PlayerStatusEffect,PlayerStatusData>()
	{
		
		{PlayerStatusEffect.AMBERED,  							new PlayerStatusData("AMBERED",icon: 3,stun: true)									},
		{PlayerStatusEffect.PLANTED,  							new PlayerStatusData("PLANTED",icon: 3,stun:true)									},
		{PlayerStatusEffect.STUNNED, 							new PlayerStatusData("STUNNED",icon:  3,stun: true)									},
		{PlayerStatusEffect.PARALYZED,							new PlayerStatusData("PARALYZED",icon:  3,stun: true)								},
		{PlayerStatusEffect.GRABBED,  							new PlayerStatusData("GRABBED",icon:  3,stun: true)									},
		{PlayerStatusEffect.CRINGE,  							new PlayerStatusData("CRINGE",icon: 3,stun: true)									},
		{PlayerStatusEffect.DEAD ,    							new PlayerStatusData("DEAD",icon:  7,remove: false,stun: true)						},
		{PlayerStatusEffect.POISONED, 							new PlayerStatusData("POISONED",icon:  0,remove: false)								},
		{PlayerStatusEffect.BURNING, 							new PlayerStatusData("BURNING",icon: 1,remove: false)								},
		{PlayerStatusEffect.REVERSED,							new PlayerStatusData("REVERSED",icon: 4)											},
		{PlayerStatusEffect.FLIGHT,   							new PlayerStatusData("FLIGHT",icon: 12, self: true)									},
		{PlayerStatusEffect.INVULN,   							new PlayerStatusData("INVULN",icon: 9, remove: false, self: true)					},
		{PlayerStatusEffect.ARMOUR,  							new PlayerStatusData("ARMOUR",icon: 10, remove: false, self: true)					},
		{PlayerStatusEffect.EXPOSED,							new PlayerStatusData("EXPOSED",icon: 2,channel: 1)									},
		{PlayerStatusEffect.FEATHERWEIGHT,						new PlayerStatusData("FEATHERWEIGHT",icon: 2,remove: false, channel: 1)				},
		{PlayerStatusEffect.SLOWED,  							new PlayerStatusData("SLOWED",icon: 11,remove: false,channel: 3)					},
		{PlayerStatusEffect.SPEEDUP, 							new PlayerStatusData("SPEEDUP",icon: 5,remove: false,self: true, channel: 3)		},
		{PlayerStatusEffect.DAMAGEUP,							new PlayerStatusData("DAMAGEUP",icon: 6,remove: false,self: true, channel: 4)		},
		{PlayerStatusEffect.DEFENSEUP,							new PlayerStatusData("DEFENSEUP",icon: 13,remove: false,self :true, channel: 5)		},
		{PlayerStatusEffect.DEFENSEDOWN,						new PlayerStatusData("DEFENSEDOWN",icon: 8,remove: false, channel: 5)				},
		{PlayerStatusEffect.HIT,    							new PlayerStatusData("HIT")															},
		{PlayerStatusEffect.END_LAG,							new PlayerStatusData("END_LAG",stun: true, self: true)								},
		{PlayerStatusEffect.KNOCKBACK,							new PlayerStatusData("KNOCKBACK",remove: false, stun: true)							},
		{PlayerStatusEffect.HITPAUSE, 							new PlayerStatusData("HITPAUSE",stun: true, self:true)								},
        {PlayerStatusEffect.GUARDBROKEN,                        new PlayerStatusData("GUARDBROKEN",icon: 14,remove: false)                           },
	};


    public string name = "HIT";
    public int iconIndex = -1;
    public bool removeOnHit = true;
    public bool isStun = false;
    public bool isSelfInflicted = false;
    public int channel = 0;
    public GameObject statusBar = null;
    //public bool isMashable = false;

    PlayerStatusData(string statusName, int icon = -1 ,bool remove = true,bool stun = false, bool self = false, int channel = 0) //, bool mashable = false)
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
    float time = 0f;
    Dictionary<PlayerStatusData, float> statusEffects = new Dictionary<PlayerStatusData, float>();
    Rigidbody2D rb;
    Drifter drifter;
    Vector2 delayedVelocity;


    int combocount = 0;

    public PlayerCard card;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        drifter = GetComponent<Drifter>();
    }
 
    // Update is called once per frame
    void Update()
    {
        if(time > .1f){
            time = 0f;
            //Hitpause pauses all other statuses for its duration
            if(HasStatusEffect(PlayerStatusEffect.HITPAUSE))
            {
                 statusEffects[PlayerStatusData.statusDataMap[PlayerStatusEffect.HITPAUSE]]--;
                 if(!HasStatusEffect(PlayerStatusEffect.HITPAUSE))rb.velocity = delayedVelocity;
            }
            else{
                List<PlayerStatusData> keys = new List<PlayerStatusData>(statusEffects.Keys);
				foreach(PlayerStatusData key in keys)
        		{
                    if(HasStatusEffect(key) && statusEffects[key] > 0)
                    {
                        statusEffects[key]--;
                        if(key == PlayerStatusData.statusDataMap[PlayerStatusEffect.BURNING]) drifter.DamageTaken += .2f;
                        if(key == PlayerStatusData.statusDataMap[PlayerStatusEffect.CRINGE] && !HasStatusEffect(PlayerStatusEffect.CRINGE))rb.velocity = delayedVelocity;
                    }
                    else{
                        statusEffects[key] = 0;
                    }
                }
            }
            
        }

        if(!HasEnemyStunEffect() && combocount >0)
        {
        	UnityEngine.Debug.Log("COMBO DROPPED at :" + combocount);
        	combocount = 0;

        }
        time += Time.deltaTime;
        
    }

    //Returns true if the player has the specified status and its duration is not zero
    public bool HasStatusEffect(PlayerStatusEffect ef)
    {
        return HasStatusEffect(PlayerStatusData.statusDataMap[ef]);
    }

    bool HasStatusEffect(PlayerStatusData ef)
    {
    	return statusEffects.ContainsKey(ef) && statusEffects[ef] > 0;
    }

    //Returns true if player is not actionable
    public bool HasStunEffect()
    {
    	foreach(KeyValuePair<PlayerStatusData,float> ef in statusEffects)
    	{
    		if(HasStatusEffect(ef.Key) && ef.Key.isStun) return true;
    	}
    	return false;
    }

    //Returns true if player is not actionable by an enemy's hand
    public bool HasEnemyStunEffect()
    {
    	foreach(KeyValuePair<PlayerStatusData,float> ef in statusEffects)
    	{
    		if(HasStatusEffect(ef.Key) && ef.Key.isStun && !ef.Key.isSelfInflicted) return true;
    	}
    	return false;

        
    }

    //Player is not in hitstun
    public bool HasGroundFriction()
    {
        return !HasStatusEffect(PlayerStatusEffect.KNOCKBACK);
    }

    public void ApplyStatusEffect(PlayerStatusEffect ef, float duration)
    {
        ApplyStatusEffectFor(ef, duration);
    }

    //Clears all removable Status effects
    public void clearRemoveOnHitStatus()
    {

    	List<PlayerStatusData> keys = new List<PlayerStatusData>(statusEffects.Keys);
		foreach(PlayerStatusData key in keys)
		{
    		if(statusEffects.ContainsKey(key) && key.removeOnHit ) statusEffects[key] = 0f;
    	}
    }

	//Clears ALL status effects    
    public void clearAllStatus()
    {
        List<PlayerStatusData> keys = new List<PlayerStatusData>(statusEffects.Keys);
		foreach(PlayerStatusData key in keys)
        {
        	if(statusEffects.ContainsKey(key))statusEffects[key] = 0f;
        }
    }

    //Clears ALL status effects on a given status channel    
    public void clearStatusChannel(int channel)
    {
        List<PlayerStatusData> keys = new List<PlayerStatusData>(statusEffects.Keys);
		foreach(PlayerStatusData key in keys)
        {
        	if(statusEffects.ContainsKey(key) && key.channel == channel)statusEffects[key] = 0f;
        }
    }

    //Gets the remaining duration for a given stats effect
    public float remainingDuration(PlayerStatusEffect ef)
    {
    	return remainingDuration(PlayerStatusData.statusDataMap[ef]);
    }

    float remainingDuration(PlayerStatusData ef)
    {
    	if(statusEffects.ContainsKey(ef)) return statusEffects[ef];
    	else return 0;
    }

    //Reduces hitstun duration when restituted
    public void bounce()
    {
        if(HasStatusEffect(PlayerStatusEffect.KNOCKBACK)){
            statusEffects[PlayerStatusData.statusDataMap[PlayerStatusEffect.KNOCKBACK]] *= .8f;
        }
    }

    //Called once per frame if the player is mashing; Reduces remaining duration of effects
    //TODO make mashable a parameter?
    public void mashOut(){
        //Adjust these numbers to make it easier or harder to mash out
        if(HasStatusEffect(PlayerStatusEffect.AMBERED))statusEffects[PlayerStatusData.statusDataMap[PlayerStatusEffect.AMBERED]]-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.PLANTED))statusEffects[PlayerStatusData.statusDataMap[PlayerStatusEffect.PLANTED]]-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.PARALYZED))statusEffects[PlayerStatusData.statusDataMap[PlayerStatusEffect.PARALYZED]]-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.GRABBED))statusEffects[PlayerStatusData.statusDataMap[PlayerStatusEffect.GRABBED]]-=.4f;
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
    GameObject addStatusBar(PlayerStatusEffect ef, float duration)
    {
    	if(card==null || PlayerStatusData.statusDataMap[ef].iconIndex < 0)return null;

    	return card.addStatusBar(ef,PlayerStatusData.statusDataMap[ef].iconIndex,duration,this);
    }


    //God this bullshit...
    void ApplyStatusEffectFor(PlayerStatusEffect ef, float duration)
    {
    	PlayerStatusData data = PlayerStatusData.statusDataMap[ef];

    	if(PlayerStatusData.statusDataMap[ef].isStun && !PlayerStatusData.statusDataMap[ef].isSelfInflicted)
    	{

    		if(ef == PlayerStatusEffect.DEAD && combocount > 0)
    		{
    			
    			UnityEngine.Debug.Log(drifter.drifterType + " got bodied in " + combocount + " hits!");
    			combocount = 0;

    		}
    		else if(ef == PlayerStatusEffect.DEAD)
    		{
    			combocount = 0;
    		}
    		else
    		{
    			combocount++;
    			UnityEngine.Debug.Log(combocount);
    		}
    	}

    	if(!HasStatusEffect(ef) && PlayerStatusData.statusDataMap[ef].statusBar == null) PlayerStatusData.statusDataMap[ef].statusBar = addStatusBar(ef,duration);
    	//Ignores hitstun if in superarmour or invuln
        if(ef == PlayerStatusEffect.DEAD){
            clearAllStatus();
            statusEffects[data] = duration * 10f;
            return;
        }

        if(data.channel != 0)clearStatusChannel(data.channel);

        if(ef == PlayerStatusEffect.PARALYZED){
            rb.velocity = new Vector2(0,15f);
        }

    	if((HasStatusEffect(PlayerStatusEffect.INVULN) || HasStatusEffect(PlayerStatusEffect.ARMOUR)) && data.isStun && !data.isSelfInflicted){
    		return;
    	}

        if((data.isStun && !data.isSelfInflicted && HasStatusEffect(ef))|| (HasStatusEffect(PlayerStatusEffect.PLANTED) && (ef == PlayerStatusEffect.GRABBED))){
            statusEffects[PlayerStatusData.statusDataMap[PlayerStatusEffect.KNOCKBACK]] = 5.5f;
            clearRemoveOnHitStatus();
            return;
        }
        //If youre planted or stunned, you get unplanted by a hit

        if((ef == PlayerStatusEffect.KNOCKBACK || data.isStun && !data.isSelfInflicted))clearRemoveOnHitStatus();        

        if (!statusEffects.ContainsKey(data))
        {
            statusEffects[data] = 0f;
        }
        
        if(ef == PlayerStatusEffect.HITPAUSE || ef == PlayerStatusEffect.CRINGE){
            //save delayed velocity

            delayedVelocity = rb.velocity;
        }


    	statusEffects[data] = duration * 10f;

    }
}
