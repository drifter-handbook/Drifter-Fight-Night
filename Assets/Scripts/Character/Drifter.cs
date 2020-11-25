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

    public void OnNetworkInit()
    {
        NetworkUtils.RegisterChildObject("PlayerAnimator", transform.Find("Sprite").gameObject);
        NetworkUtils.RegisterChildObject("PlayerStatusController", transform.Find("PlayerStatusController").gameObject);
    }

    public void Awake()
    {
        sync = GetComponent<NetworkSync>();
        status = GetComponent<PlayerStatus>();
    }

    public void Start()
    {
        Stocks = 3;
        DamageTaken = 0f;
    }

    public Color GetColor()
    { 
        return transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().color;
    }

    public void SetPeerId(int id){
        peerID = id;
        myColor = CharacterMenu.ColorFromEnum[(PlayerColor)(peerID>0?peerID:0)];
        transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().color = myColor;
        transform.GetChild(3).GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),myColor);
    }

    // used by host
    public void SetAnimatorTrigger(string s)
    {
        if (GameController.Instance.IsHost)
        {
            animator.gameObject.GetComponent<SyncAnimatorHost>().SetTrigger(s);
        }
    }
    public void SetAnimatorBool(string s, bool value)
    {
        animator.SetBool(s, value);
    }

    public DrifterType GetDrifterType(){
        return DrifterTypeFromString(drifterType);
    }

    public static DrifterType DrifterTypeFromString(String drfiterString){
        return (DrifterType)Enum.Parse(typeof(DrifterType), drfiterString.Replace(" ", "_"));
    }
}
