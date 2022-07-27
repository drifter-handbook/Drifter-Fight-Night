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
    HIDDEN,
    FLATTEN,
    TUMBLE,
    KNOCKDOWN,
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

public class PlayerStatus : MonoBehaviour, INetworkMessageReceiver
{
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
        {PlayerStatusEffect.POISONED,                           new PlayerStatusData("POISONED",icon:  0,remove: false)                             },
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
        {PlayerStatusEffect.GUARDCRUSHED,                       new PlayerStatusData("GUARDCRUSHED",icon: 14)                                       },
        {PlayerStatusEffect.STANCE,                             new PlayerStatusData("STANCE",remove: false,self: true)                             },
        {PlayerStatusEffect.SLOWMOTION,                         new PlayerStatusData("SLOWMOTION",icon: 16)                                         },
        {PlayerStatusEffect.HIDDEN,                             new PlayerStatusData("HIDDEN",remove: false)                                        },
        {PlayerStatusEffect.TUMBLE,                             new PlayerStatusData("TUMBLE")                                                      },
        {PlayerStatusEffect.KNOCKDOWN,                          new PlayerStatusData("KNOCKDOWN", icon: 3,stun: true)                               },
        {PlayerStatusEffect.FLATTEN,                            new PlayerStatusData("FLATTEN")                                                     },
    };

    Rigidbody2D rb;
    Drifter drifter;
    Vector2 delayedVelocity;

    // float frameAdvantage = 0;
    public bool isInCombo = false;

    public PlayerCard card;
    [SerializeField] private PlayerDamageNumbers damageDisplay;

    public Collider2D grabPoint = null;

    PlayerStatusEffect delayedEffect;
    int delayedEffectDuration;

    // Start is called before the first frame update
    void Start()
    {
    	sync = GetComponent<NetworkSync>();
        rb = GetComponent<Rigidbody2D>();
        drifter = GetComponent<Drifter>();
        if(!GameController.Instance.IsTraining)ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,111);
    }
 
    // Update is called once per frame
    void FixedUpdate()
    {
        if(GameController.Instance.IsPaused)
            return;

        if(grabPoint!=null && HasStatusEffect(PlayerStatusEffect.GRABBED) && grabPoint.enabled)
        {
            //rb.position = grabPoint.bounds.center;
            drifter.transform.position = grabPoint.bounds.center;
        }

        else if(HasStatusEffect(PlayerStatusEffect.GRABBED) && (grabPoint== null || !grabPoint.enabled))
        {
            grabPoint=null;
            statusDataMap[PlayerStatusEffect.GRABBED].duration = 0;
            rb.velocity = delayedVelocity;
            delayedVelocity = Vector2.zero;
        }
        
            //Hitpause pauses all other statuses for its duration
            if(HasStatusEffect(PlayerStatusEffect.HITPAUSE) || HasStatusEffect(PlayerStatusEffect.GRABBED))
            {
                statusDataMap[PlayerStatusEffect.HITPAUSE].duration--;
                if(!HasStatusEffect(PlayerStatusEffect.HITPAUSE) && !HasStatusEffect(PlayerStatusEffect.SLOWMOTION))
                {
                    if(delayedVelocity != Vector2.zero)rb.velocity = delayedVelocity;
                    if(delayedEffect != PlayerStatusEffect.HIT)
                    {
                        ApplyStatusEffect(delayedEffect,delayedEffectDuration);
                        delayedEffect = PlayerStatusEffect.HIT;
                    }
                }
            }
            //Otherwise, tick down all active statuses
            else{

                foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
                {
                    if(HasStatusEffect(ef.Key))
                    {
                        ef.Value.duration--;

                        //Damage player if they are on fire
                        if(ef.Key == PlayerStatusEffect.BURNING) drifter.DamageTaken += Time.fixedDeltaTime;
                        
                        //Re-apply the saved velocity if the player just lost cringe
                        
                        if(ef.Key == PlayerStatusEffect.SLOWMOTION && HasEnemyStunEffect())rb.velocity = delayedVelocity * .2f;


                        if((ef.Key == PlayerStatusEffect.CRINGE && !HasStatusEffect(PlayerStatusEffect.CRINGE)) || (ef.Key == PlayerStatusEffect.SLOWMOTION && !HasStatusEffect(PlayerStatusEffect.SLOWMOTION)))
                        {
                            rb.velocity = delayedVelocity;
                            drifter.SetAnimationSpeed(1f);
                        }

                        if(ef.Key == PlayerStatusEffect.FLATTEN)
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
        foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
        {
            if(HasStatusEffect(ef.Key) && ef.Value.isStun && !ef.Value.isSelfInflicted) return true;
        }
        return false;        
    }

    public bool hasAdditionalStunEffect()
    {
        foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
        {
            if(HasStatusEffect(ef.Key) && ef.Value.isStun && !ef.Value.isSelfInflicted && ef.Key != PlayerStatusEffect.KNOCKBACK  && ef.Key != PlayerStatusEffect.HITPAUSE) return true;
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
        return !HasStatusEffect(PlayerStatusEffect.KNOCKBACK);
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
		foreach(KeyValuePair<PlayerStatusEffect,PlayerStatusData> ef in statusDataMap)
        {
            if(ef.Value.removeOnHit) 
                ef.Value.duration = 0;
        
        }
        drifter.SetAnimationSpeed(1f);
        grabPoint = null;
    }


    //Clears all stun Status effects
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
    public int remainingDuration(PlayerStatusEffect ef)
    {
    	return statusDataMap[ef].duration;
    }

    //Reduces hitstun duration when restituted
    public void bounce()
    {
        if(HasStatusEffect(PlayerStatusEffect.KNOCKBACK)){
            statusDataMap[PlayerStatusEffect.KNOCKBACK].duration = (int)(statusDataMap[PlayerStatusEffect.KNOCKBACK].duration * .8f);
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

    	return card.addStatusBar(ef,statusDataMap[ef].iconIndex,duration,this);
    }


    //God this bullshit...
    void ApplyStatusEffectFor(PlayerStatusEffect ef, int duration)
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

        if(ef == PlayerStatusEffect.PARALYZED)rb.velocity = new Vector2(0,15f);

    	if((HasStatusEffect(PlayerStatusEffect.INVULN) || HasStatusEffect(PlayerStatusEffect.ARMOUR)) && data.isStun && !data.isSelfInflicted) return;
    

        //Disallow unique stuns on already stunned opponents.
        //TODO See if this is necessary when plants are reintroduced
        if((data.isStun && !data.isSelfInflicted && HasStatusEffect(ef)) && ef != PlayerStatusEffect.KNOCKBACK || (HasStatusEffect(PlayerStatusEffect.PLANTED) && (ef == PlayerStatusEffect.GRABBED))){
            
            statusDataMap[PlayerStatusEffect.KNOCKBACK].duration = 30;
            clearRemoveOnHitStatus();
            return;
        }
        //If youre planted or stunned, you get unplanted by a hit

        if((ef == PlayerStatusEffect.KNOCKBACK || data.isStun && !data.isSelfInflicted) && duration >0)clearRemoveOnHitStatus();        
        
        //save delayed velocity
        if((ef == PlayerStatusEffect.HITPAUSE || ef == PlayerStatusEffect.CRINGE || ef == PlayerStatusEffect.GRABBED || ef == PlayerStatusEffect.SLOWMOTION ||(ef == PlayerStatusEffect.KNOCKBACK &&  HasStatusEffect(PlayerStatusEffect.SLOWMOTION)))&& rb.velocity != Vector2.zero) delayedVelocity = rb.velocity;

        //Slow down animation speed in slowmo
        if(ef == PlayerStatusEffect.SLOWMOTION)
            drifter.SetAnimationSpeed(.4f);

        if(data.isStun && !data.isSelfInflicted) isInCombo = true;

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
    	public int statusDuration;
	}


}
