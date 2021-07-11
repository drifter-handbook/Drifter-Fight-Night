using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MythariusMasterHit : MasterHit
{
 	GameObject slowfield;

    PROJECTILE_TYPE prev_projectile = PROJECTILE_TYPE.sugarbeet;

    public TetherRange sacredFlameDetector;

    enum PROJECTILE_TYPE
    {
        beet,
        mail,
        bird,
        chilltouch,
        rayoffrost,
        sugarbeet,
        pkmeteor,
        sacredFlame
    }

    void Update()
    {
        if(!isHost)return;

        if(movement.terminalVelocity != terminalVelocity  && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        {
            resetTerminal();
        }

        //Allow for Down W if stunned
        if(Empowered && !status.HasStatusEffect(PlayerStatusEffect.END_LAG) && drifter.input.Special && drifter.input.MoveY < 0)
        {
        	fightOrFlight();
        }
    }


    public void enableSacredFlameDetector()
    {
        if(!isHost)return;
        sacredFlameDetector.gameObject.SetActive(true);
    }

    public void W_Side_Projectile()
    {
        if(!isHost)return;

        facing = movement.Facing;

        PROJECTILE_TYPE projectile = (PROJECTILE_TYPE)Random.Range(0,8);

        //Disallow the same projectile twice in a row
        while(projectile == prev_projectile) projectile = (PROJECTILE_TYPE)Random.Range(0,8);

        prev_projectile = projectile;

        Vector3 pos = transform.position +  new Vector3(facing *1.8f, 4.5f, 0f);
        Vector3 velocity = Vector3.zero;

        switch(projectile){
            case PROJECTILE_TYPE.beet:
                velocity = new Vector3(facing * 20f,0,0);
                break;
            case PROJECTILE_TYPE.mail:
            case PROJECTILE_TYPE.chilltouch:
                velocity = new Vector3(facing * 35f,0,0);
                break;
            case PROJECTILE_TYPE.rayoffrost:
                velocity = new Vector3(facing * 25f,0,0);
                break;
            case PROJECTILE_TYPE.bird:
                pos = transform.position + new Vector3(facing, 7.5f, 0f);
                velocity = new Vector3(facing * 15f,0,0);
                break;
            
            case PROJECTILE_TYPE.sugarbeet:
                velocity = new Vector3(facing * 15f,0,0);
                break;

            case PROJECTILE_TYPE.pkmeteor:
                velocity = new Vector3(facing * 15f,0,0);
                break;
            case PROJECTILE_TYPE.sacredFlame:
                if(sacredFlameDetector.TetherPoint == Vector3.zero)
                {
                    sacredFlameDetector.gameObject.SetActive(false);
                    GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.SmokeTrail, pos, transform.rotation.eulerAngles.z,new Vector2(1, 1));
                    return;
                }
                else pos = sacredFlameDetector.TetherPoint;
                break;    
            default:
                break;

        }

        GameObject wildcard = host.CreateNetworkObject(projectile.ToString(), pos, transform.rotation);

        sacredFlameDetector.gameObject.SetActive(false);

        if(projectile == PROJECTILE_TYPE.bird && facing == -1) wildcard.GetComponent<SyncAnimatorStateHost>().SetState("birb_Reverse");

        wildcard.GetComponent<Rigidbody2D>().velocity = velocity;
        wildcard.transform.localScale = new Vector3( wildcard.transform.localScale.x * facing,wildcard.transform.localScale.y);
        foreach (HitboxCollision hitbox in wildcard.GetComponentsInChildren<HitboxCollision>(true))
        {
            if(projectile != PROJECTILE_TYPE.sugarbeet) hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID + 150;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }

    }

	//Down W

    public void counter()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.HIT)){
            drifter.PlayAnimation("W_Down_Success");
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.2f);
        }
    }


    public void spawnSlowZone()
    {

    	if(!isHost)return;

        Vector3 pos = new Vector3(0f, 4.5f, 0f);
        //TODO Add delete animation here
        if (slowfield)Destroy(slowfield);
        slowfield = host.CreateNetworkObject("myth_slowfield", transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in slowfield.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID + 150;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }

        
        Empowered = true;
        drifter.SetCharge(4);
    }

    public void fightOrFlight()
    {
    	if(!isHost || !Empowered)return;
        drifter.SetCharge(0);
    	Empowered = false;
   
        //GP orange
        status.clearAllStatus();
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,3f);
        drifter.PlayAnimation("W_Down_Boost");

        status.ApplyStatusEffect(PlayerStatusEffect.SPEEDUP,5f);
        status.ApplyStatusEffect(PlayerStatusEffect.DAMAGEUP,3f);

    }

    // IEnumerator resetSpeed()
    // {
    // 	for(int i = 0; i < 40;i++)
    // 	{
    // 		GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Heal, transform.position + new Vector3(UnityEngine.Random.Range(-2f,2f), UnityEngine.Random.Range(1.5f,6f)), 0, new Vector2(1, 1));
    // 		yield return new WaitForSeconds(.175f);
    // 	}
    // 	movement.walkSpeed = 23.5f;
    // 	movement.airSpeed = 23.5f;
    // }

    //Up W

    public void upWGlide()
    {
        if(!isHost)return;

        if(cancelAttack())
        {
        	resetTerminal();
        }

        else if(drifter.input.MoveY <0 || movement.grounded)
        {
        	resetTerminal();
        	returnToIdle();
        }
        else
        {

        	movement.updateFacing();
        	rb.velocity = new Vector2(Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)? drifter.input.MoveX * 23f:(.6f*23f)),rb.velocity.x,.75f),rb.velocity.y);
            movement.updateFacing();
            movement.terminalVelocity = 10f;
        }
    }

    public void resetTerminal()
    {
    	if(!isHost)return;
        movement.terminalVelocity = terminalVelocity;
    }

    //Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0,75f,0);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.42f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,5f);
    }

}
