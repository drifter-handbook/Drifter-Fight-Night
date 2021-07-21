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
    Sandbag,
}



/** 
 * This is the class that will be put into a prefab and instantiated 
 */
[RequireComponent(typeof(PlayerMovement))]
public class Drifter : MonoBehaviour, INetworkInit
{
    public PlayerMovement movement;

    public PlayerInputData input;

    //Input Buffer
    public PlayerInputData[] prevInput;// = new PlayerInputData[32];
    //public PlayerInputData prevInput;

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

    //REMOVE THIS
    //int Charge = 0;

    float terminalVelocity;


    public float superCharge = 2f;

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
    [NonSerialized]
    public bool canFeint = true;

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
        Stocks = !GameController.Instance.IsTraining ? 4:999;
        if(drifterType=="Sandbag")SetColor(8);
        DamageTaken = 0f;

        terminalVelocity = movement.terminalVelocity;
    }

    //Returns the character's outline color as an int
    public int GetColor()
    { 
        return myColor;
    }

    //Sets the character's super charge to a given value
    public void SetCharge(float newCharge)
    { 
        if(superCharge != newCharge) superCharge=newCharge;
        if(isHost) gameObject.GetComponent<SyncChargeHost>().setCharge(superCharge);
    }

    //Grants the character additonal charge for their super meter, up to the cap of 5 bars
    public void gainSuperMeter(float charge)
    {
        if(isHost)
            superCharge = Mathf.Min(charge + superCharge,5f);
    }


    //Stes Peerid for networking
    public void SetPeerId(int id){
        peerID = id;
    }

    //Sets the character's outline color
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

    //Flips text-based objects attacked to characters to keep them readable as the character turns
    public void SetIndicatorDirection(float facing)
    {
        if(isHost)
            {
                transform.GetChild(0).localScale = new Vector2(Mathf.Abs(transform.GetChild(0).localScale.x) * facing,transform.GetChild(0).localScale.y);
                transform.GetChild(4).localScale = new Vector2(Mathf.Abs(transform.GetChild(4).localScale.x) * facing,transform.GetChild(4).localScale.y);
            }
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

    //Returns whihc animation layer is currently being used
    public int GetAnimationLayer()
    {
        return animationLayer;
    }

    //Switches animation layer for use in stance charactrs
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
        canFeint = true;
        clearGuardFlags();
        if(movement.grounded)animator.gameObject.GetComponent<SyncAnimatorStateHost>().SetState(GroundIdleStateName,animationLayer);
        else animator.gameObject.GetComponent<SyncAnimatorStateHost>().SetState(AirIdleStateName,animationLayer);
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
        movement.terminalVelocity = terminalVelocity;
        
    }



    //Returns the remaining time in the current animation in seconds for use in frame data calculations
    public float getRemainingAttackTime()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(animationLayer);

        return status.HasStatusEffect(PlayerStatusEffect.END_LAG) ? info.length *  (1f - info.normalizedTime +  Mathf.Floor(info.normalizedTime)) : 0;
    }

    //Clears all flags associated with guard state
    public void clearGuardFlags()
    {
        guarding = false;
        parrying = false;
        perfectGuarding = false;
        guardBreaking = false;
    }


    //Command Input Detection

    //Detects if the character dobule tapped the X directional key
    public bool doubleTappedX()
    {
        if(input.MoveX ==0)return false;

        int state = 0;

        for(int i = 0; i < prevInput.Length-5; i++)
        {

            if(state ==0 && prevInput[i].MoveX == 0)
                state++;
            else if(state == 1 && prevInput[i].MoveX == -1 * input.MoveX) return false;

            else if(state == 1 && prevInput[i].MoveX == input.MoveX)
                return true;
        }

        return false;
    }

    //Detects if the character dobule tapped the Y directional key
    public bool doubleTappedY()
    {
        if(input.MoveY ==0)return false;

        int state = 0;

        for(int i = 0; i < prevInput.Length-5; i++)
        {

            if(state ==0 && prevInput[i].MoveY == 0)
                state++;
            else if(state == 1 && prevInput[i].MoveY == -1 * input.MoveY) return false;

            else if(state == 1 && prevInput[i].MoveY == input.MoveY)
                return true;
        }

        return false;
    }

    //Detects if the character executed a Quater Circle motion
    //Sign indicates direction, number indicated the number of times executed
    //in the case of ties, such as 1 QCF and one QCB, the most recent command will be returned
    public int quarterCircle()
    {
        int QCS = 0;
        for(int i = 0; i < prevInput.Length; i++)
        {

        }
        return QCS;
    }


    public DrifterType GetDrifterType(){
        return DrifterTypeFromString(drifterType);
    }

    public static DrifterType DrifterTypeFromString(String drfiterString){
        return (DrifterType)Enum.Parse(typeof(DrifterType), drfiterString.Replace(" ", "_"));
    }
}
