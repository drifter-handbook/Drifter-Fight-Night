using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum DrifterType
{
    None,
    Bojo,
    Swordfrog,
    Lady_Parhelion,
    Spacejam,
    Orro,
    Ryyke,
    Megurin,
    Nero,
    Random,
    Lucille,
    Mytharius,
    Maryam,
    Drifter_Cannon,
    Klatz,
    Orro_Classic
}



/** 
 * This is the class that will be put into a prefab and instantiated 
 */
[RequireComponent(typeof(PlayerMovement))]
public class Drifter : MonoBehaviour, INetworkInit
{
    public PlayerMovement movement;

    public PlayerInputData input;
    public PlayerInputData prevInput;

    public Animator animator;

    NetworkSync sync;
    public int Stocks {
        get { return (int)sync["stocks"]; }
        set { sync["stocks"] = value; }
    }
    public float DamageTaken {
        get { return (float)sync["damageTaken"]; }
        set { sync["damageTaken"] = value; }
    }

    int Charge = 0;

    public float BlockReduction = .5f;
    
    public int myColor;

    public String drifterType;

    public int peerID;

    public PlayerStatus status;

    bool isHost;

    //
    [NonSerialized]
    public string GroundIdleStateName = "Idle";
    [NonSerialized]
    public string AirIdleStateName = "Hang";
    [NonSerialized]
    public string GuardStateName = "Guard";
    [NonSerialized]
    public string WalkStateName = "Walk";
    [NonSerialized]
    public string JumpStartStateName = "Jump_Start";
    [NonSerialized]
    public string JumpEndStateName = "Jump_End";
    [NonSerialized]
    public string WeakLedgeGrabStateName = "Ledge_Grab_Weak";
    [NonSerialized]
    public string StrongLedgeGrabStateName = "Ledge_Grab_Strong";
    [NonSerialized]
    public string LedgeClimbStateName = "Ledge_Climb";
    [NonSerialized]
    public string LedgeRollStateName = "Ledge_Roll";

    public int animationLayer = 0;
    
    
    //public bool forceGuard = false;

    //[NonSerialized]
    public bool guarding = false;
    [NonSerialized]
    public bool perfectGuarding = false;
    [NonSerialized]
    public bool parrying = false;
    [NonSerialized]
    public bool guardBreaking = false;

    public void OnNetworkInit()
    {
        NetworkUtils.RegisterChildObject("PlayerAnimator", transform.Find("Sprite").gameObject);
        NetworkUtils.RegisterChildObject("PlayerStatusController", transform.Find("PlayerStatusController").gameObject);
        NetworkUtils.RegisterChildObject("PlayerNumberIndicator", transform.Find("PlayerIndicator").gameObject);
    }

    public void Awake()
    {
        isHost = GameController.Instance.IsHost;
        sync = GetComponent<NetworkSync>();
        status = GetComponent<PlayerStatus>();
    }

    public void Start()
    {
        Stocks = drifterType!="Sandbag"?3:999;
        if(drifterType=="Sandbag")SetColor(8);
        DamageTaken = 0f;
    }

    public int GetColor()
    { 
        return myColor;
    }

    public void SetCharge(int newCharge)
    { 
        if(Charge != newCharge) Charge=newCharge;
        if(isHost) gameObject.GetComponent<SyncChargeHost>().setCharge(Charge);
    }

    public void ModifyCharge(int newCharge)
    { 
        if(isHost) gameObject.GetComponent<SyncChargeHost>().setCharge(Charge + newCharge);
    }

    public void IncrementCharge()
    { 
        Charge++;
        if(isHost) gameObject.GetComponent<SyncChargeHost>().setCharge(Charge);
    }

    public void DecrementCharge()
    { 
        Charge--;
        if(isHost) gameObject.GetComponent<SyncChargeHost>().setCharge(Charge);
    }

    public int GetCharge()
    { 
        return this.Charge;
    }


    public void SetPeerId(int id){
        peerID = id;
        //myColor = CharacterMenu.ColorFromEnum[(PlayerColor)(peerID>0?peerID:0)];
        //transform.GetChild(0).GetComponent<SpriteRenderer>().color = myColor;
        //transform.GetChild(3).GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),myColor);
    }

    public void SetColor(int colorID)
    {
        myColor = (colorID>=0?colorID:0);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = CharacterMenu.ColorFromEnum[(PlayerColor)myColor];
        transform.GetChild(3).GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)myColor]);
        if(isHost){
            transform.GetChild(0).GetComponent<SyncAnimatorStateHost>().SetState("P" + (colorID + 1));
            gameObject.GetComponent<SyncColorDataHost>().setColor(myColor);
        }
    }

    public void SetIndicatorDirection(float facing)
    {
        if(isHost)transform.GetChild(0).localScale = new Vector2(Mathf.Abs(transform.GetChild(0).localScale.x) * facing,transform.GetChild(0).localScale.y);
    }

    //Replaces the animator state transition function
    public void PlayAnimation(string state)
    {
        if(!isHost)return;
        if(Animator.StringToHash(state) != animator.GetCurrentAnimatorStateInfo(animationLayer).fullPathHash)
        {
            animator.gameObject.GetComponent<SyncAnimatorStateHost>().SetState(state,animationLayer);
        }
        else
        {
            UnityEngine.Debug.Log("FAILED TO PLAY STATE; STATE IS ALREADY PLAYING: " + state);
        }
    }

    public int GetAnimationLayer()
    {
        return animationLayer;
    }

    public void SetAnimationLayer(int layer)
    {
        animationLayer = layer;

        for(int i = 0; i < animator.layerCount; i++)
        {
            if(i == layer)animator.SetLayerWeight(i,1);
            else animator.SetLayerWeight(i,0);
        }
        
        if(!isHost)return;
        animator.gameObject.GetComponent<SyncAnimatorLayerHost>().SetLayer(layer);

    }

    //Return to idle is called anytime the player regains control
    public void returnToIdle()
    {
        //UnityEngine.Debug.Log("DRIFTER: RETURNING TO IDLE: " + state);
        movement.canLandingCancel = false;
        clearGuardFlags();
        if(movement.grounded)animator.gameObject.GetComponent<SyncAnimatorStateHost>().SetState(GroundIdleStateName,animationLayer);
        else animator.gameObject.GetComponent<SyncAnimatorStateHost>().SetState(AirIdleStateName,animationLayer);
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
        
    }

    public float getRemainingAttackTime()
    {

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(animationLayer);

        return status.HasStatusEffect(PlayerStatusEffect.END_LAG) ? info.length *  (1f - info.normalizedTime +  Mathf.Floor(info.normalizedTime)) : 0;

    }

    public void clearGuardFlags()
    {
        guarding = false;
        parrying = false;
        perfectGuarding = false;
        guardBreaking = false;
    }

    public DrifterType GetDrifterType(){
        return DrifterTypeFromString(drifterType);
    }

    public static DrifterType DrifterTypeFromString(String drfiterString){
        return (DrifterType)Enum.Parse(typeof(DrifterType), drfiterString.Replace(" ", "_"));
    }
}
