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
    GUARDCRUSHED,
    STANCE,
    SLOWMOTION, 
}

class PlayerStatusData
{
    public string name = "HIT";
    public int iconIndex = -1;
    public bool removeOnHit = true;
    public bool isStun = false;
    public bool isSelfInflicted = false;
    public int channel = 0;
    public GameObject statusBar = null;
    public float duration = 0;
    public bool stacking = false;
    public float stacks = 1f;
    //public bool isMashable = false;

    public PlayerStatusData(string statusName, int icon = -1 ,bool remove = true,bool stun = false, bool self = false, int channel = 0, bool stacking = false) //, bool mashable = false)
    { 
        name = statusName;
        iconIndex = icon;
        removeOnHit = remove;
        isStun = stun;
        isSelfInflicted = self;
        this.channel = channel;
        this.stacking = stacking;

        //isMashable = mashable;
    }
}

public class PlayerStatus : MonoBehaviour, INetworkMessageReceiver
{

	static float framerateScalar =.0833333333f;

	NetworkSync sync;

    Dictionary<PlayerStatusEffect,PlayerStatusData> statusDataMap = new Dictionary<PlayerStatusEffect,PlayerStatusData>()
    {
        
        {PlayerStatusEffect.AMBERED,                            new PlayerStatusData("AMBERED",icon: 3,stun: true)                                  },
        {PlayerStatusEffect.PLANTED,                            new PlayerStatusData("PLANTED",icon: 3,stun:true)                                   },
        {PlayerStatusEffect.STUNNED,                            new PlayerStatusData("STUNNED",icon:  3,stun: true)                                 },
        {PlayerStatusEffect.PARALYZED,                          new PlayerStatusData("PARALYZED",icon:  3,stun: true)                               },
        {PlayerStatusEffect.GRABBED,                            new PlayerStatusData("GRABBED",icon:  3,stun: true)                                 },
        {PlayerStatusEffect.CRINGE,                             new PlayerStatusData("CRINGE",icon: 3,stun: true)                                   },
        {PlayerStatusEffect.DEAD ,                              new PlayerStatusData("DEAD",icon:  7,remove: false,stun: true)                      },
        {PlayerStatusEffect.POISONED,                           new PlayerStatusData("POISONED",icon:  0,remove: false,stacking:true)               },
        {PlayerStatusEffect.BURNING,                            new PlayerStatusData("BURNING",icon: 1,remove: false)                               },
        {PlayerStatusEffect.REVERSED,                           new PlayerStatusData("REVERSED",icon: 4)                                            },
        {PlayerStatusEffect.FLIGHT,                             new PlayerStatusData("FLIGHT",icon: 12, self: true)                                 },
        {PlayerStatusEffect.INVULN,                             new PlayerStatusData("INVULN",icon: 9, remove: false, self: true)                   },
        {PlayerStatusEffect.ARMOUR,                             new PlayerStatusData("ARMOUR",icon: 10, remove: false, self: true)                  },
        {PlayerStatusEffect.EXPOSED,                            new PlayerStatusData("EXPOSED",icon: 2,channel: 1)                                  },
        {PlayerStatusEffect.FEATHERWEIGHT,                      new PlayerStatusData("FEATHERWEIGHT",icon: 2,remove: false, channel: 1)             },
        {PlayerStatusEffect.SLOWED,                             new PlayerStatusData("SLOWED",icon: 11,remove: false,channel: 3)                    },
        {PlayerStatusEffect.SPEEDUP,                            new PlayerStatusData("SPEEDUP",icon: 5,remove: false,self: true, channel: 3)        },
        {PlayerStatusEffect.DAMAGEUP,                           new PlayerStatusData("DAMAGEUP",icon: 6,remove: false,self: true, channel: 4)       },
        {PlayerStatusEffect.DEFENSEUP,                          new PlayerStatusData("DEFENSEUP",icon: 13,remove: false,self :true, channel: 5)     },
        {PlayerStatusEffect.DEFENSEDOWN,                        new PlayerStatusData("DEFENSEDOWN",icon: 8,remove: false, channel: 5)               },
        {PlayerStatusEffect.HIT,                                new PlayerStatusData("HIT")                                                         },
        {PlayerStatusEffect.END_LAG,                            new PlayerStatusData("END_LAG",stun: true, self: true)                              },
        {PlayerStatusEffect.KNOCKBACK,                          new PlayerStatusData("KNOCKBACK",remove: false, stun: true)                         },
        {PlayerStatusEffect.HITPAUSE,                           new PlayerStatusData("HITPAUSE",stun: true, self:true)                              },
        {PlayerStatusEffect.GUARDCRUSHED,                       new PlayerStatusData("GUARDCRUSHED",icon: 14)                         },
        {PlayerStatusEffect.STANCE,                             new PlayerStatusData("STANCE",remove: false,self: true)                             },
        {PlayerStatusEffect.SLOWMOTION,                         new PlayerStatusData("SLOWMOTION",icon: 16)                                         },
    };

    // float time = 0f;
    Rigidbody2D rb;
    Drifter drifter;
    Vector2 delayedVelocity;

    float frameAdvantage = 0;
    public bool isInCombo;

    int combocount = 0;

    public PlayerCard card;
    public TrainingUIManager trainingUI;
    [SerializeField] private PlayerDamageNumbers damageDisplay;

    public Collider2D grabPoint = null;
    // Start is called before the first frame update
    void Start()
    {
    	sync = GetComponent<NetworkSync>();
        rb = GetComponent<Rigidbody2D>();
        drifter = GetComponent<Drifter>();
        if(!GameController.Instance.IsTraining)ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,1.85f);
    }
 
    // Update is called once per frame
    void Update()
    {
        if(GameController.Instance.IsPaused)
            return;

        if(grabPoint!=null && HasStatusEffect(PlayerStatusEffect.GRABBED) && grabPoint.enabled)rb.position = grabPoint.bounds.center;

        else if(HasStatusEffect(PlayerStatusEffect.GRABBED) && (grabPoint== null || !grabPoint.enabled))
        {
            grabPoint=null;
            statusDataMap[PlayerStatusEffect.GRABBED].duration = 0;
            rb.velocity = delayedVelocity;
            delayedVelocity = Vector2.zero;
        }
        
        // if(time >= .f)
        // {
        //     time = 0f;
            //Hitpause pauses all other statuses for its duration
            if(HasStatusEffect(PlayerStatusEffect.HITPAUSE) || HasStatusEffect(PlayerStatusEffect.GRABBED))
            {
                 statusDataMap[PlayerStatusEffect.HITPAUSE].duration-= Time.deltaTime;
                 if(!HasStatusEffect(PlayerStatusEffect.HITPAUSE) && !HasStatusEffect(PlayerStatusEffect.SLOWMOTION) && delayedVelocity != Vector2.zero)rb.velocity = delayedVelocity;
            }
            //Otherwise, tick down all active statuses
            else{

                foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
                {
                    if(HasStatusEffect(ef.Key))
                    {
                        if(ef.Key == PlayerStatusEffect.STANCE)
                            continue;

                        else ef.Value.duration -= Time.deltaTime;
                        //Damage player if they are on fire
                        if(ef.Key == PlayerStatusEffect.BURNING) drifter.DamageTaken += Time.deltaTime;
                        
                        //Re-apply the saved velocity if the player just lost cringe
                        
                        if(ef.Key == PlayerStatusEffect.SLOWMOTION && HasEnemyStunEffect())rb.velocity = delayedVelocity * .2f;


                        if((ef.Key == PlayerStatusEffect.CRINGE && !HasStatusEffect(PlayerStatusEffect.CRINGE)) || (ef.Key == PlayerStatusEffect.SLOWMOTION && !HasStatusEffect(PlayerStatusEffect.SLOWMOTION)))
                        {
                            rb.velocity = delayedVelocity;
                            drifter.SetAnimationSpeed(1f);
                        }


                        //if(ef.Key == PlayerStatusEffect.ORBO && !HasStatusEffect(ef.Key)) 

                        if(!HasStatusEffect(ef.Key))ef.Value.stacks=0f;

                    }
                }
            }
            
        //}
        if(delayedVelocity != Vector2.zero && !(HasStatusEffect(PlayerStatusEffect.HITPAUSE) || HasStatusEffect(PlayerStatusEffect.CRINGE) || HasStatusEffect(PlayerStatusEffect.GRABBED) || HasStatusEffect(PlayerStatusEffect.SLOWMOTION))) delayedVelocity = Vector2.zero;

        //If you are actionable, end combo
        if(!HasEnemyStunEffect() && combocount >0)
        {
        	//UnityEngine.Debug.Log("COMBO DROPPED at :" + combocount);
        	combocount = 0;
        	frameAdvantage = 0;
            isInCombo = false;

        }
        //time += Time.deltaTime;
        
    }

    //Returns true if the player has the specified status and its duration is not zero
    public bool HasStatusEffect(PlayerStatusEffect ef)
    {
        return statusDataMap[ef].duration > 0;
    }

    //Returns true if player is not actionable
    public bool HasStunEffect()
    {

    	foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
    	{
    		if(HasStatusEffect(ef.Key) && ef.Value.isStun) return true;
    	}
    	return false;
    }

    //Returns true if player is not actionable by an enemy's hand
    public bool HasEnemyStunEffect()
    {
        foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
        {
            if(HasStatusEffect(ef.Key) && ef.Value.isStun && !ef.Value.isSelfInflicted) return true;
        }
        return false;        
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
        return !HasStatusEffect(PlayerStatusEffect.KNOCKBACK);
    }

    public void ApplyStatusEffect(PlayerStatusEffect ef, float duration)
    {
        ApplyStatusEffectFor(ef, duration);
    }

    public void ApplyDamage(float damage, bool isCombo, float hitstun) {
        damageDisplay.Increment(damage, isCombo, hitstun);
    }

    //Clears all removable Status effects
    public void clearRemoveOnHitStatus()
    {
		foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
        {
            if(ef.Value.removeOnHit) 
                ef.Value.duration = 0;
        
        }
        drifter.SetAnimationSpeed(1f);
        grabPoint = null;
    }


    //Clears all removable Status effects
    public void clearStunStatus()
    {
        foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
        {
            if(ef.Value.isStun)
                ef.Value.duration = 0;

        }
        grabPoint = null;
    }

	//Clears ALL status effects    
    public void clearAllStatus()
    {
        foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
            ef.Value.duration = 0;

        grabPoint = null;
        drifter.SetAnimationSpeed(1f);
    }

    //Clears ALL status effects on a given status channel    
    public void clearStatusChannel(int channel)
    {
        foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
        {
            if(ef.Value.channel == channel)
                ef.Value.duration = 0;


        }
    }

    public void clearVelocity()
    {
        delayedVelocity = Vector2.zero;
    }

    //Gets the remaining duration for a given stats effect
    public float remainingDuration(PlayerStatusEffect ef)
    {
    	return statusDataMap[ef].duration;
    }

    //Reduces hitstun duration when restituted
    public void bounce()
    {
        if(HasStatusEffect(PlayerStatusEffect.KNOCKBACK)){
            statusDataMap[PlayerStatusEffect.KNOCKBACK].duration *= .8f;
        }
    }


    // //Called by playerHurtboxHandler to calculate frame advantage on hit.
    public void calculateFrameAdvantage(float defeander,float attacker)
    {
    	frameAdvantage =  ((defeander - attacker ) / framerateScalar * 12);
    }

    //Called once per frame if the player is mashing; Reduces remaining duration of effects
    //TODO make mashable a parameter?
    public void mashOut(){
        //Adjust these numbers to make it easier or harder to mash out
        if(HasStatusEffect(PlayerStatusEffect.AMBERED))statusDataMap[PlayerStatusEffect.AMBERED].duration-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.PLANTED))statusDataMap[PlayerStatusEffect.PLANTED].duration-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.PARALYZED))statusDataMap[PlayerStatusEffect.PARALYZED].duration-=.4f;
        if(HasStatusEffect(PlayerStatusEffect.GRABBED))statusDataMap[PlayerStatusEffect.GRABBED].duration-=.4f;
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
    	if(card==null)return null;

    	return card.addStatusBar(ef,statusDataMap[ef].iconIndex,duration,this);
    }


    //God this bullshit...
    void ApplyStatusEffectFor(PlayerStatusEffect ef, float duration)
    {

    	PlayerStatusData data = statusDataMap[ef];

    	if(GameController.Instance.IsHost && !(data.iconIndex < 0))
        {
        	sync.SendNetworkMessage(new PlayerStatusPacket()
            {
                effect = ef,
                statusDuration = duration,
            });
        }

    	//Combo Counter
    	if(data.isStun && !data.isSelfInflicted)
    	{

    		if(ef == PlayerStatusEffect.DEAD && combocount > 0)
    		{
    			
    			//UnityEngine.Debug.Log(drifter.drifterType + " got bodied in " + combocount + " hits!");
    			combocount = 0;
                isInCombo = false;
                damageDisplay.Reset();
    		}
    		else if(ef == PlayerStatusEffect.DEAD)
    		{
    			combocount = 0;
                isInCombo = false;
                damageDisplay.Reset();
    		}
    		else
    		{
    			combocount++;
    			//UnityEngine.Debug.Log(combocount + " Hit; " + (frameAdvantage > 0 ?"+":"" ) + frameAdvantage.ToString("0.0"));
                trainingUI.WriteCombo(combocount);
                trainingUI.WriteFrame((int)frameAdvantage);
    			frameAdvantage = 0;
                isInCombo = true;
    		}
    	}

        //If status effect stacks, add new duration to current duration and return.
        if(data.stacking && HasStatusEffect(ef))
        {
            data.stacks += duration;
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

        if(ef == PlayerStatusEffect.PARALYZED)rb.velocity = new Vector2(0,15f);

    	if((HasStatusEffect(PlayerStatusEffect.INVULN) || HasStatusEffect(PlayerStatusEffect.ARMOUR)) && data.isStun && !data.isSelfInflicted) return;
    

        if((data.isStun && !data.isSelfInflicted && HasStatusEffect(ef)) && ef != PlayerStatusEffect.KNOCKBACK|| (HasStatusEffect(PlayerStatusEffect.PLANTED) && (ef == PlayerStatusEffect.GRABBED))){
            
            statusDataMap[PlayerStatusEffect.KNOCKBACK].duration = .55f;
            clearRemoveOnHitStatus();
            return;
        }
        //If youre planted or stunned, you get unplanted by a hit

        if((ef == PlayerStatusEffect.KNOCKBACK || data.isStun && !data.isSelfInflicted))clearRemoveOnHitStatus();        
        
        //save delayed velocity
        if((ef == PlayerStatusEffect.HITPAUSE || ef == PlayerStatusEffect.CRINGE || ef == PlayerStatusEffect.GRABBED || ef == PlayerStatusEffect.SLOWMOTION ||(ef == PlayerStatusEffect.KNOCKBACK &&  HasStatusEffect(PlayerStatusEffect.SLOWMOTION)))&& rb.velocity != Vector2.zero) delayedVelocity = rb.velocity;

        //Slow down animation speed in slowmo
        if(ef == PlayerStatusEffect.SLOWMOTION)
            drifter.SetAnimationSpeed(.4f);

    	data.duration = duration;

    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        if (!GameController.Instance.IsHost)
        {
            PlayerStatusPacket statusPacket = NetworkUtils.GetNetworkData<PlayerStatusPacket>(message.contents);
            if (statusPacket != null) ApplyStatusEffectFor(statusPacket.effect, statusPacket.statusDuration);
        }
    }


	public class PlayerStatusPacket : INetworkData
	{
    	public string Type { get; set; }
    	public PlayerStatusEffect effect;
    	public float statusDuration;
	}


}
