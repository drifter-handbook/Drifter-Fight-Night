using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstone : MonoBehaviour
{
    Rigidbody2D rb;
    public Animator anim;
    public int facing = 0;
    bool armed = false;
    GameObject Ryyke;
    public PlayerAttacks attacks;
    bool grounded = false;
    bool broken = false;
    bool isHost = false;

    void Awake()
    {
        isHost = GameController.Instance.IsHost;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!isHost)return;
        rb = GetComponent<Rigidbody2D>();
        if(!armed)rb.velocity = new Vector2(0f,-50f);
        Ryyke = gameObject.GetComponentInChildren<HitboxCollision>().parent;
        attacks = Ryyke.GetComponent<PlayerAttacks>();
    }

    public void Break()
    {
        if(!isHost)return;
        if(grounded)anim.Play("Grounded_Delete");
        else anim.Play("Aerial_Delete");
    }

    public void arm()
    {
        if(!isHost)return;
        armed = true;
    }

    public void awakenActivate()
    {
        if(!isHost)return;
        armed = true;
        anim.Play("Raw_Activate");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!isHost)return;  
        if ((col.gameObject.tag == "Ground" || col.gameObject.tag == "Platform") && !broken && !armed)
        {
            grounded = true;  
            anim.Play("Place");
            rb.velocity=Vector2.zero;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if(!isHost)return;
        if(!armed && col.gameObject.tag == "Player" && col.gameObject != GetComponentInChildren<HitboxCollision>().parent){
            broken = true;
            anim.Play("Aerial_Delete");
            rb.velocity=Vector2.zero;
        }

        else if(!armed && col.gameObject.layer == 9 && GetComponentInChildren<HitboxCollision>().parent != col.gameObject.GetComponent<HitboxCollision>().parent)
        {
            broken = true;
            anim.Play("Aerial_Delete");
            rb.velocity=Vector2.zero;
        }

        else if(armed && col.gameObject != Ryyke && col.gameObject.tag == "Player") //&& col.gameObject != hitbox.parent)
        {
            //activate = true;
            anim.Play("Activate");
        }
    }

    public void PlayAnimation(string state){
        if(!isHost)return;
        anim.Play(state);
    }

    public void SpawnChad()
    {
        if(!isHost)return;
        Vector3 flip = new Vector3(facing * 12f, 12f, 1f);
        GameObject zombie = GameController.Instance.host.CreateNetworkObject("Chadwick", transform.position, transform.transform.rotation);

        try
        {
            zombie.transform.localScale = flip;
            foreach (HitboxCollision hitbox in zombie.GetComponentsInChildren<HitboxCollision>(true))
            {
                attacks.SetupAttackID(DrifterAttackType.W_Down);
                hitbox.parent = Ryyke;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = DrifterAttackType.W_Down;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
            Ryyke.GetComponentInChildren<RykkeMasterHit>().grantStack();
        }
        catch (NullReferenceException E)
        {
                //I'm sick of this shit
        }
    }  
}
