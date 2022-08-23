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

/** 
 * This is the class that will be put into a prefab and instantiated 
 */
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerAttacks))]
public class Drifter : MonoBehaviour
{
    //Static values durring gameplay
    public PlayerMovement movement;
    public PlayerStatus status;
    public PlayerAttacks attacks;
    public MasterHit masterhit;
    public PlayerHurtboxHandler hurtbox;
    
    public Animator animator;
    public AnimatorOverrideController[] animOverrides;
    public Animator sparkle;
    public PlayerInput playerInputController;


    public DrifterType drifterType;

    [NonSerialized]
    public int myColor;
    [NonSerialized]
    public int peerID;
    
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


    public void Start()
    {
        Stocks = !GameController.Instance.IsTraining ? 4:9999;
        DamageTaken = 0f;

        if(animOverrides != null && animOverrides.Length > 0)animOverrides[0] = new AnimatorOverrideController(animator.runtimeAnimatorController);
        masterhit = GetComponentInChildren<MasterHit>();

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
        //gameObject.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)myColor]);
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
    public void PlayAnimation(string p_state, float p_normalizedTime = -1, bool p_gate = false)
    {
        if(p_gate && Animator.StringToHash(p_state) == animator.GetCurrentAnimatorStateInfo(0).shortNameHash)
        {
            UnityEngine.Debug.Log("Animation state " +p_state + " was gated!");
        }
        else
        {
            animator.Play(Animator.StringToHash(p_state),0,p_normalizedTime < 0 ? 0: p_normalizedTime);
        }
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
        if(movement.grounded && input[0].MoveX !=0)PlayAnimation("Walk");
        else if(movement.grounded)PlayAnimation("Idle");
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


    //Rollback
    //====================================

    //Takes a snapshot of the current frame to rollback to
    public DrifterRollbackFrame SerializeFrame()
    {
        return new DrifterRollbackFrame()
        {

            //Input buffer
            InputBuffer = input,

            //Character State
            Guarding = guarding,
            PerfectGuarding = perfectGuarding,
            Parrying = parrying,
            GuardBreaking = guardBreaking,
            CanFeint = canFeint,
            CanSuper = canSuper,
            KnockedDown = knockedDown,
            CanSpecialCancel = _canSpecialCancel, 
            Hidden = hiddenFlag,
            SuperCharge = superCharge,
            Stocks = this.Stocks,
            DamageTaken = this.DamageTaken,
            CancelTimer = cancelTimer,
            ListenForSpecialCancel = listenForSpecialCancel,
            SparkleMode = sparkleMode,

            //Animation
            AnimationOverrideIndex = overrideIndex,
            AnimationSpeed = animator.speed,
            AnimationClip = animator.GetCurrentAnimatorStateInfo(0).shortNameHash,
            AnimationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime,
            AnimatorEnabled = animator.enabled,

            //Components
            MovementFrame = movement.SerializeFrame(),
            AttackFrame = attacks.SerializeFrame(),
            MasterhitFrame =  masterhit.SerializeFrame(),
            StatusFrame = status.SerializeFrame(),
            HurtboxhitFrame = hurtbox.SerializeFrame(),
        };
    }

    //Rolls back the entity to a given frame state
    public  void DeserializeFrame(DrifterRollbackFrame p_frame)
    {

        //Input buffer
        input = p_frame.InputBuffer;

        //Character State
        guarding = p_frame.Guarding;
        perfectGuarding = p_frame.PerfectGuarding;
        parrying = p_frame.Parrying;
        guardBreaking = p_frame.GuardBreaking;
        canFeint = p_frame.CanFeint;
        canSuper = p_frame.CanSuper;
        knockedDown = p_frame.KnockedDown;
        _canSpecialCancel = p_frame.CanSpecialCancel; 
        hiddenFlag = p_frame.Hidden;
        superCharge = p_frame.SuperCharge;
        Stocks = p_frame.Stocks;
        DamageTaken = p_frame.DamageTaken;
        cancelTimer = p_frame.CancelTimer;
        listenForSpecialCancel = p_frame.ListenForSpecialCancel;
        sparkleMode = p_frame.SparkleMode;

        //Animation
        animator.enabled = p_frame.AnimatorEnabled;
        SetAnimationOverride(p_frame.AnimationOverrideIndex); 
        animator.speed = p_frame.AnimationSpeed;
        animator.Play(p_frame.AnimationClip,0,p_frame.AnimationTime);
        
        //Components
        movement.DeserializeFrame(p_frame.MovementFrame);
        attacks.DeserializeFrame(p_frame.AttackFrame);
        masterhit.DeserializeFrame(p_frame.MasterhitFrame);
        status.DeserializeFrame(p_frame.StatusFrame);
        hurtbox.DeserializeFrame(p_frame.HurtboxhitFrame);

    }

    public void UpdateFrame()
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
        movement.UpdateFrame();
        attacks.UpdateFrame();
        masterhit.UpdateFrame();
        status.UpdateFrame();
        hurtbox.UpdateFrame();


        //Serialize frame
        SerializeFrame();
        
    }
}

public class DrifterRollbackFrame: INetworkData
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
    public StatusRollbackFrame StatusFrame;
    public HurtboxRollbackFrame HurtboxhitFrame;
    
}
