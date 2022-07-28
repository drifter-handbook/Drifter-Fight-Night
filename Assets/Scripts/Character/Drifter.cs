using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public enum DrifterType
{
    None,
    Random,
    Sandbag,
    Bojo,
    Swordfrog,
    Lady_Parhelion,
    Spacejam,
    Orro,
    Ryyke,
    Megurin,
    Nero,
    Lucille,
    Mytharius,
    Maryam,
    Drifter_Cannon,
    //Klatz,
    //Eldaris,
    //Reed,
    //Bytor,
    //Dyo,
    //Ramstein,
    //Tai,
    //Tasma,
    //Sola,
    //Oono,

}

public class DrifterRollbackFrame: INetworkData, ICloneable
{
    public string Type { get; set; }
    public PlayerInputData[] InputBuffer;
    
    public bool Guarding;
    public bool PerfectGuarding;
    public bool Parrying;
    public bool GuardBreaking;
    public bool CanFeint;
    public bool CanSuper;
    public bool KnockedDown;
    public bool CanSpecialCancel; 
    public bool Hidden;
    public float SuperCharge;
    public int Stocks;
    public float DamageTaken;
    
    public int CancelTimer;
    public bool ListenForSpecialCancel;
    public bool SparkleMode;

    public int AnimationOverrideIndex; 
    public float AnimationSpeed;
    public int AnimationClip;
    public float AnimationTime;
    public bool AnimatorEnabled;

    public MovementRollbackFrame MovementFrame;
    public AttackRollbackFrame AttackFrame;
    public MasterhitRollbackFrame MasterhitFrame;

    public object Clone()
    {
        return new MovementRollbackFrame()
        {};
    }
}



/** 
 * This is the class that will be put into a prefab and instantiated 
 */
[RequireComponent(typeof(PlayerMovement))]
public class Drifter : MonoBehaviour
{
    //Static values durring gameplay
    public PlayerMovement movement;
    public PlayerStatus status;
    public PlayerAttacks attacks;
    
    public Animator animator;

    public AnimatorOverrideController[] animOverrides;
    public int myColor;
    public DrifterType drifterType;
    public int peerID;
    
    public Animator sparkle;
    public PlayerInput playerInputController;

    //Serializeable values
    
    //Input Buffer
    public PlayerInputData[] input;

    // [NonSerialized]
    // public float animationSpeed = 1;
    [NonSerialized]
    public bool guarding = false;
    [NonSerialized]
    public bool perfectGuarding = false;
    [NonSerialized]
    public bool parrying = false;
    [NonSerialized]
    public bool guardBreaking = false;
    [NonSerialized]
    public bool canFeint = true;
    [NonSerialized]
    public bool canSuper = true;
    [NonSerialized]
    public bool knockedDown = false;
    [NonSerialized]
    public bool canSpecialCancelFlag = false; //True when a move has connected but the player has not yet canceled their move
    [NonSerialized]
    public bool hiddenFlag = false;
    [NonSerialized]
    public float superCharge = 2f;
    [NonSerialized]
    public bool sparkleMode = false;

    public int Stocks;
    public float DamageTaken;
    
    private int overrideIndex = 0; 
    private int cancelTimer = 0;
    private bool _canSpecialCancel = false;
    //private int animatorClipHash;
    //private float animatorTime;

    //Cancel Normals into Specials Logic
    public bool listenForSpecialCancel
    {
        get{
            return _canSpecialCancel;
        }
        set{
            _canSpecialCancel = value;
            cancelTimer = _canSpecialCancel ? 18:0;
            //sparkle.SetState(_canSpecialCancel?"ChargeIndicator":"Hide");
        }
    }

    

    // public void OnNetworkInit()
    // {
    //     NetworkUtils.RegisterChildObject("PlayerAnimator", transform.Find("Sprite").gameObject);
    //     NetworkUtils.RegisterChildObject("PlayerNumberIndicator", transform.Find("PlayerIndicator").gameObject);
    // }

    // public void Awake()
    // {
    //     isHost = GameController.Instance.IsHost;
    //     sync = GetComponent<NetworkSync>();
    //     status = GetComponent<PlayerStatus>();
    // }

    public void Start()
    {
        Stocks = !GameController.Instance.IsTraining ? 4:9999;
        DamageTaken = 0f;

        if(animOverrides != null && animOverrides.Length > 0)animOverrides[0] = new AnimatorOverrideController(animator.runtimeAnimatorController);
    }

    void FixedUpdate()
    {
        if(cancelTimer >0)
        {
            cancelTimer--;
            if(cancelTimer <=0)
            {
                cancelTimer = 0;
                listenForSpecialCancel = false;
                canSpecialCancelFlag = false;
            }
        }
        
    }

    public bool canSpecialCancel()
    {
        return (canSpecialCancelFlag && listenForSpecialCancel && cancelTimer > 0);
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
        //gameObject.GetComponent<SyncChargeHost>().setCharge(superCharge);
    }

    //Grants the character additonal charge for their super meter, up to the cap of 5 bars
    public void gainSuperMeter(float charge)
    {
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
        transform.GetChild(2).GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)myColor]);
        gameObject.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)myColor]);
        transform.GetChild(0).GetComponent<Animator>().Play( (colorID < 8)?"P" + (colorID + 1):"P9");
    }

    public void toggleHidden(bool hidden)
    {
        if(hiddenFlag == hidden) return;
        hiddenFlag = hidden;
        SpriteRenderer render = transform.GetChild(2).GetComponent<SpriteRenderer>();
        Color newColor = render.color;
        newColor.a = hidden?0:1;
        render.color = newColor;
        //transform.GetChild(2).GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)myColor]);
    }

    //Flips text-based objects attacked to characters to keep them readable as the character turns
    public void SetIndicatorDirection(float facing)
    {
        transform.GetChild(0).localScale = new Vector2(Mathf.Abs(transform.GetChild(0).localScale.x) * facing,transform.GetChild(0).localScale.y);
        transform.GetChild(3).localScale = new Vector2(Mathf.Abs(transform.GetChild(3).localScale.x) * facing,transform.GetChild(3).localScale.y);
    }


    public void Sparkle(bool p_mode)
    {
        if(sparkleMode ==p_mode) return;

        sparkleMode = p_mode;
        sparkle.Play(p_mode?"ChargeIndicator":"Hide");
    }

    //Replaces the animator state transition function
    public void PlayAnimation(string p_state, float p_normalizedTime = -1)
    {
        // if(!isHost)return;
        if(Animator.StringToHash(p_state) == animator.GetCurrentAnimatorStateInfo(-1).fullPathHash && p_normalizedTime == -1)
            UnityEngine.Debug.Log("DUPLICATE STATE CALL MADE: " + p_state);
        
        else
            PlayAnimation(Animator.StringToHash(p_state),p_normalizedTime < 0 ? 0: p_normalizedTime);
        

        // animator.Play(p_state,,-1,p_normalizedTime);
        // animatorClipHash = animator.GetCurrentAnimatorStateInfo(animationLayer).fullPathHash;
        // animatorTime = p_normalizedTime;

        
    }

    private void PlayAnimation(int p_state, float p_normalizedTime)
    {
        animator.Play(p_state,-1,p_normalizedTime);
        // animatorClipHash = p_state;
        // animatorTime = p_normalizedTime;
    }



    public void SetAnimationOverride(int p_index)
    {
        if(animOverrides.Length == null || animOverrides.Length < p_index+1)
        {
            UnityEngine.Debug.LogWarning("No animation override set for index: " + p_index);
            return;
        }
        animator.runtimeAnimatorController = animOverrides[p_index];
        overrideIndex = p_index;
    }

    public void SetAnimationSpeed(float p_speed)
    {
        animator.speed = p_speed;
    }

    public void ToggleAnimator(bool p_mode)
    {
        animator.enabled = p_mode;
    }



    //Return to idle is called anytime the player regains control
    public void returnToIdle()
    {
        //UnityEngine.Debug.Log("DRIFTER: RETURNING TO IDLE: " + state);
        movement.canLandingCancel = false;
        movement.jumping = false;
        movement.dashing = false;
        movement.canFastFall = true;
        canFeint = true;
        canSuper = true;
        clearGuardFlags();
        if(movement.grounded)PlayAnimation("Idle");
        else PlayAnimation("Hang");
        if(status.HasStatusEffect(PlayerStatusEffect.END_LAG)) status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0);
        if(status.HasStatusEffect(PlayerStatusEffect.FLATTEN)) status.ApplyStatusEffect(PlayerStatusEffect.FLATTEN,0);
        if(status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN))  status.ApplyStatusEffect(PlayerStatusEffect.KNOCKDOWN,0);
        movement.resetTerminalVelocity();
        canSpecialCancelFlag = false;
        listenForSpecialCancel = false;     
        //knockedDown = false;
        knockedDown = false;
        if(transform.position.z != -1) transform.position = new Vector3(transform.position.x,transform.position.y,-1);

        if(input[0].Guard)
        {
            guarding = true;
            PlayAnimation(movement.hitstun?"Guard":"Guard_Start");
        }
        movement.hitstun = false;
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

        if(input[0].MoveX ==0)return false;

        int state = 0;

        for(int i = 1; i < input.Length-6; i++)
        {
            if(state ==0 && input[i].MoveX == 0)
                state++;
            else if(state == 1 && input[i].MoveX == -1 * input[0].MoveX)
                return false;
            else if(state == 1 && input[i].MoveX == input[0].MoveX)
                return true;
        }

        return false;
    }

    //Detects if the character dobule tapped the Y directional key
    public bool doubleTappedY()
    {
        if(input[0].MoveY ==0)return false;

        int state = 0;

        for(int i = 1; i < input.Length-6; i++)
        {

            if(state ==0 && input[i].MoveY == 0)
                state++;
            else if(state == 1 && input[i].MoveY == -1 * input[0].MoveY) return false;

            else if(state == 1 && input[i].MoveY == input[0].MoveY)
                return true;
        }

        return false;
    }

    //Detects if the character executed a Quater Circle motion
    public bool qcf()
    {
        
        if(input[0].MoveX ==0 || input[0].MoveY !=0 )return false;

        int state = 0;
        for(int i = 1; i < input.Length; i++)
        {
            if(state ==0 && input[i].MoveY == -1 && input[i].MoveX == input[0].MoveX)
                state++;
            else if(state == 1 && input[i].MoveY == 0 && input[i].MoveX == input[0].MoveX) 
                state--;

            else if(state == 1 && input[i].MoveY == -1 && input[i].MoveX ==0)
                return true;


        }
        return false;
    }

    // public float GetCurrentAnimationRemainder() {
    //     AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(animationLayer);
    //     return (1 - (info.normalizedTime - (int)info.normalizedTime)) * info.length;
    // }

  
    // public DrifterType GetDrifterType(){
    //     return DrifterTypeFromString(drifterType);
    // }

    public static DrifterType DrifterTypeFromString(String drfiterString){
        return (DrifterType)Enum.Parse(typeof(DrifterType), drfiterString.Replace(" ", "_"));
    }
}
