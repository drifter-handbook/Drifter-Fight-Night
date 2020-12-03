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

    public int Charge = 0;

    public float BlockReduction = .5f;
    
    public Color myColor;

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
    
    //public bool forceGuard = false;

    [NonSerialized]
    public bool guarding = false;

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
        DamageTaken = 0f;
    }

    public Color GetColor()
    { 
        return transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().color;
    }

    public void SetPeerId(int id){
        peerID = id;
        //myColor = CharacterMenu.ColorFromEnum[(PlayerColor)(peerID>0?peerID:0)];
        //transform.GetChild(0).GetComponent<SpriteRenderer>().color = myColor;
        //transform.GetChild(3).GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),myColor);
    }

    public void SetColor(int colorID)
    {

        UnityEngine.Debug.Log(colorID);
        myColor = CharacterMenu.ColorFromEnum[(PlayerColor)(colorID>0?colorID:0)];
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = myColor;
        transform.GetChild(3).GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),myColor);
        if(isHost)transform.GetChild(0).GetComponent<SyncAnimatorStateHost>().SetState("P" + (colorID + 1));
    }

    public void SetIndicatorDirection(float facing)
    {
        if(isHost)transform.GetChild(0).localScale = new Vector2(Mathf.Abs(transform.GetChild(0).localScale.x) * facing,transform.GetChild(0).localScale.y);
    }

    //Replaces the animator state transition function
    public void PlayAnimation(string state)
    {
        if(!isHost)return;
        if(Animator.StringToHash(state) != animator.GetCurrentAnimatorStateInfo(0).fullPathHash)
        {
            animator.gameObject.GetComponent<SyncAnimatorStateHost>().SetState(state);
        }
    }

    //Return to idle is called anytime the player regains control
    public void returnToIdle()
    {
        movement.canLandingCancel = false;
        if(movement.grounded)animator.gameObject.GetComponent<SyncAnimatorStateHost>().SetState(GroundIdleStateName);
        else animator.gameObject.GetComponent<SyncAnimatorStateHost>().SetState(AirIdleStateName);
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
    }

    public DrifterType GetDrifterType(){
        return DrifterTypeFromString(drifterType);
    }

    public static DrifterType DrifterTypeFromString(String drfiterString){
        return (DrifterType)Enum.Parse(typeof(DrifterType), drfiterString.Replace(" ", "_"));
    }
}
