using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinMasterHit : MasterHit
{
    public SpriteRenderer sprite;
    GameObject activeStorm;
    Vector2 HeldDirection;

    float terminalVelocity;

    int neutralWCharge = 0;
    public float lightningCharge = 0f;
    public float windCharge = 0f;
    public float iceCharge = 0f;
    float elementChargeMax = 30f;


    void Start()
    {
        terminalVelocity = movement.terminalVelocity;
    }


    void Update()
    {
        //Reset charges on death
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD)){
            lightningCharge = 0;
            windCharge = 0;
            iceCharge = 0;
        }
    }

    
    //Recovery Logic  

    public void saveDirection(){
        if(!isHost)return;
        Vector2 TestDirection = new Vector2(drifter.input.MoveX,drifter.input.MoveY);
        HeldDirection = TestDirection == Vector2.zero? HeldDirection: TestDirection;
    }


    public void recoveryWarpStart(){

        HeldDirection.Normalize();

        movement.terminalVelocity = 225f;

        rb.velocity = 225f* HeldDirection;

    }

    public void resetTerminalVelocity(){

        movement.terminalVelocity = terminalVelocity;

    }


    //Projectiles

    public void Dair(){
        if(!isHost)return;
        GameObject dairBolt = host.CreateNetworkObject("MegurinDairBolt", transform.position, transform.rotation);
        foreach (HitboxCollision hitbox in dairBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
    }


    public void FTilt(){
        if(!isHost)return;
        facing = movement.Facing;
        GameObject windwave = host.CreateNetworkObject("Windwave", transform.position + new Vector3(facing * 3f, 1f), transform.rotation);
        windwave.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 35f, 0);
        windwave.transform.localScale = new Vector3(facing * 12f, 12f);
        foreach (HitboxCollision hitbox in windwave.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        
    }

    public void Uair(){
        if(!isHost)return;
        GameObject Megunado = host.CreateNetworkObject("Megunado", transform.position + new Vector3(0, 3.3f), transform.rotation);
        Megunado.GetComponent<Rigidbody2D>().velocity = Vector3.up * 23f;
        foreach (HitboxCollision hitbox in Megunado.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
    }   

    public void spawnStorm(){
        if(!isHost)return;
        Vector3 pos = new Vector3(0f, 6.5f, 0f);
        
        GameObject MegurinStorm = host.CreateNetworkObject("MegurinStorm", transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in MegurinStorm.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID + 150;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }

        if (activeStorm)Destroy(activeStorm);
        MegurinStorm.GetComponent<MegurinStorm>().attacks = attacks;
        activeStorm = MegurinStorm;
        
    }

    public void spawnOrb(){
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing * 12f, 12f, 0f);
        Vector3 pos = new Vector3(facing * 4f, 5, 1f);
        
        GameObject MegurinOrb = host.CreateNetworkObject("ChromaticOrb", transform.position + pos, transform.rotation);
        MegurinOrb.transform.localScale = flip;
        MegurinOrb.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 25, 0);
        MegurinOrb.GetComponent<Animator>().SetInteger("Mode", drifter.Charge);
        foreach (HitboxCollision hitbox in MegurinOrb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
    }

    public void spawnSmallBolt()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing * 10f, 10f, 1f);
        Vector3 pos = new Vector3(facing * 3f, 3.3f, 1f);
        
        GameObject smallBolt = host.CreateNetworkObject("WeakBolt", transform.position + pos, transform.rotation);
        smallBolt.transform.localScale = flip;
        foreach (HitboxCollision hitbox in smallBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
            
        }
    }
    public void spawnLargeBolt()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing * 10f, 10f, 1f);
        Vector3 pos = new Vector3(facing * 3f, 3.3f, 1f);
        
        GameObject largeBolt = host.CreateNetworkObject("StrongBolt", transform.position + pos, transform.rotation);
        largeBolt.transform.localScale = flip;
        foreach (HitboxCollision hitbox in largeBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
    }


    //Elemental Logic

    public void setLightning(){
        drifter.Charge = -1;
    }
    public void setIce(){
        drifter.Charge = 1;
    }
    public void setWind(){
        drifter.Charge = 0;
    }

    public SingleAttackData handleElements(SingleAttackData attackData, int element){
        //-1 lightning
        //0 wind
        //1 ice

        switch(element){
            case -1:
            if(lightningCharge >= elementChargeMax){
                attackData.StatusEffect = PlayerStatusEffect.PARALYZED;
                attackData.StatusDuration = 3.3f;
                StartCoroutine(resetGauges());
                return attackData;
            }
            else{
                lightningCharge += attackData.AttackDamage;
                if(lightningCharge>elementChargeMax)lightningCharge = elementChargeMax;
                break;
            }
            case 0:
            if(windCharge >= elementChargeMax){
                attackData.StatusEffect = PlayerStatusEffect.FEATHERWEIGHT;
                attackData.StatusDuration = 7f;
                StartCoroutine(resetGauges());
                return attackData;
            }
            else{
                windCharge += attackData.AttackDamage;
                if(windCharge>elementChargeMax)windCharge = elementChargeMax;
                break;
            }
            case 1:
            if(iceCharge >= elementChargeMax){
                attackData.StatusEffect = PlayerStatusEffect.SLOWED;
                attackData.StatusDuration = 7f;
                StartCoroutine(resetGauges());
                return attackData;
            }
            else{
                iceCharge += attackData.AttackDamage;
                if(iceCharge>elementChargeMax)iceCharge = elementChargeMax;
                break;
            }
            default:
            break;
        }
        UnityEngine.Debug.Log("RESET");
        attackData.StatusDuration = .1f;
        attackData.StatusEffect = PlayerStatusEffect.HIT;
        return attackData;
    }

    IEnumerator resetGauges()
    {
        yield return new WaitForSeconds(.2f);
        lightningCharge = 0;
        windCharge = 0;
        iceCharge = 0;
    }


    //Neutral W Logic

    public void chargeNeutralW(int canCancel = 1)
    {
        if(!isHost)return;
        if(Empowered)
        {
            Empowered = false;
            neutralWCharge = 0;
            playState("W_Neutral_Recover_Slow");

        }
        if (canCancel != 0 &&TransitionFromChanneledAttack()){
            return;
        }
        if(canCancel != 0 && drifter.input.Special)
        {
            neutralWCharge = 0;
            playState("W_Neutral_Recover_Fast");
        }

        if(neutralWCharge < 43){
            neutralWCharge +=1;
        }
        else{
            Empowered = true;
            returnToIdle();
        }
    }

    //Inhereted Roll Methods

   public override void rollGetupStart(){
        //Unused
   }

   public override void rollGetupEnd(){
    facing = movement.Facing;
    rb.position += new Vector2(4f* facing,5f);
}

public override void roll(){
        //unused
}
}
