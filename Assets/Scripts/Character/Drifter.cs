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

    public int peerID;

    public void OnNetworkInit()
    {
        NetworkUtils.RegisterChildObject("PlayerAnimator", transform.Find("Sprite").gameObject);
        NetworkUtils.RegisterChildObject("PlayerStatusController", transform.Find("PlayerStatusController").gameObject);
        // TODO: remove when merge in austin's changes
        GameObject potentialBean = transform.Find("Sprite").Find("Bean")?.gameObject;
        if (potentialBean != null)
        {
            NetworkUtils.RegisterChildObject("Bean", potentialBean);
        }
    }

    public void Awake()
    {
        sync = GetComponent<NetworkSync>();
    }

    public void Start()
    {
        Stocks = 3;
        DamageTaken = 0f;
    }

    public void SetColor(Color color)
    {
        myColor = color;
        transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().color = color;
    }
    public Color GetColor()
    {
        return transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().color;
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
}
