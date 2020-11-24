using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MasterHit : MonoBehaviour, IMasterHit
{
    protected Drifter drifter;
    protected NetworkHost host;
    protected Rigidbody2D rb;
    protected PlayerMovement movement;
    protected PlayerStatus status;
    protected float gravityScale;
    protected PlayerAttacks attacks;
    public int facing;

    public virtual void callTheAerial()
    {

    }
    public virtual void hitTheAerial(GameObject target)
    {

    }
    public virtual void cancelTheAerial()
    {
    }

    // Start is called before the first frame update
    void Awake()
    {
        drifter = transform.parent.gameObject.GetComponent<Drifter>();
        host = GameController.Instance.host;

        //Paretn Components
        drifter = transform.parent.gameObject.GetComponent<Drifter>();
        rb = drifter.GetComponent<Rigidbody2D>();
        movement = drifter.GetComponent<PlayerMovement>();
        attacks = drifter.GetComponent<PlayerAttacks>();
        status = drifter.GetComponent<PlayerStatus>();

        gravityScale = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setYVelocity(float y)
    {
        rb.velocity = new Vector2(rb.velocity.x,y);
    }

    public void setXVelocity(float x)
    {
        rb.velocity = new Vector2(movement.Facing * x,rb.velocity.y);
    }

    public void applyEndLag(float statusDuration)
    {
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,statusDuration);
    }

    public void applyArmour(float statusDuration)
    {
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,statusDuration);
    }

    public void pauseGravity(){
        movement.gravityPaused= true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    public void unpauseGravity()
    {
        movement.gravityPaused= false;
        rb.gravityScale = gravityScale;
    }

    public void refreshHitboxID(){
        attacks.SetMultiHitAttackID();
    }

    //Allows for jump and shild canceling of moves. Returns true if it's condition was met
    public bool TransitionFromChanneledAttack()
    {
        if (!GameController.Instance.IsHost)
        {
            return false;
        }
        if(drifter.input.Guard)
        {
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
            drifter.SetAnimatorBool("Guarding", true);
            unpauseGravity();
            return true;
        }
        else if(drifter.input.Jump && movement.currentJumps>0){
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
            movement.jump();
            unpauseGravity();
            return true;
        }

        return false;
    }


    public abstract void roll();

    public abstract void rollGetupStart();
    

    public abstract void rollGetupEnd();
   
}
