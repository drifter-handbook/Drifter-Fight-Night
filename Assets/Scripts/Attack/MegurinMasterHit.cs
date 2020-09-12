using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    PlayerStatus status;
    float gravityScale;
    PlayerMovement movement;
    public Animator anim;
    public SpriteRenderer sprite;
    GameObject activeStorm;
    Vector2 HeldDirection;
    LayerMask myLayerMask;

    int neutralWCharge = 0;
    public float lightningCharge = 0f;
    public float windCharge = 0f;
    public float iceCharge = 0f;
    float elementChargeMax = 30f;

    public int facing;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
    }

    public override void callTheRecovery()
    {
        Debug.Log("Recovery start!");
    }
    public void RecoveryPauseMidair()
    {
        // pause in air
        movement.gravityPaused= true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }
    public void saveDirection(){
        Vector2 TestDirection = new Vector2(drifter.input.MoveX,drifter.input.MoveY);
        HeldDirection = TestDirection == Vector2.zero? HeldDirection: TestDirection;
    }
    private bool posTest(Vector2 pos){
         Vector2 Point;
         Vector2 Start = rb.position; // This is defined to be some arbitrary point far away from the collider.
         Vector2 Goal = pos; // This is the point we want to determine whether or not is inside or outside the collider.
         Vector2 Direction = Goal-Start; // This is the direction from start to goal.
         Direction.Normalize();
         int Itterations = 0; // If we know how many times the raycast has hit faces on its way to the target and back, we can tell through logic whether or not it is inside.
         Point = Start;
         LayerMask tempMask = ~(1 << LayerMask.NameToLayer ("Player") | 1 << LayerMask.NameToLayer ("Platform") );
         while(Point != Goal) // Try to reach the point starting from the far off point.  This will pass through faces to reach its objective.
         {
             RaycastHit2D hit = Physics2D.Linecast(Point, Goal, tempMask);
             if(hit) // Progressively move the point forward, stopping everytime we see a new plane in the way.
             {
                Itterations ++;
                Point = hit.point + (Direction/100.0f); // Move the Point to hit.point and push it forward just a touch to move it through the skin of the mesh (if you don't push it, it will read that same point indefinately).
             }
             else
             {
                 Point = Goal; // If there is no obstruction to our goal, then we can reach it in one step.
             }
         }
         if(Itterations % 2 == 0)
         {
            return false;
         }
         else
         {
            return true;
         }
    }
    public void RecoveryWarp()
    {
        saveDirection();
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.25f);
        HeldDirection.Normalize();
        Vector2 checkPosForward = rb.position + HeldDirection*21f;
        Vector2 checkPosBackward = rb.position + HeldDirection*19f;
        Vector2 checkPosNeutral = rb.position + HeldDirection*20f;
        float radius = 1.75f;
        bool resultForward = posTest(checkPosForward);
        bool resultBackward = posTest(checkPosBackward);
        bool resultNeutral = posTest(checkPosNeutral);
        bool result = resultForward && resultBackward && resultNeutral;
        if (result){
          myLayerMask = ~(1 << LayerMask.NameToLayer ("Player") | 1 << LayerMask.NameToLayer ("Platform") | 1 << LayerMask.NameToLayer ("Ground"));
          RaycastHit2D hit = Physics2D.Raycast(rb.position, HeldDirection*20f, 20, myLayerMask);
          if(hit.collider != null && hit.collider.gameObject!= null && hit.collider.gameObject.tag != "Untagged")
          {
              var distance = hit.distance-3;
              
              rb.position += HeldDirection*(distance);
          }
          else{
            rb.position += HeldDirection*20f;
          }
       }
        else{
          rb.position += HeldDirection*20f;
        }
        HeldDirection = Vector2.zero;

    }

    IEnumerator resetGauges()
    {
        yield return new WaitForSeconds(.2f);
        lightningCharge = 0;
        windCharge = 0;
        iceCharge = 0;
    }

    public SingleAttackData handleElements(SingleAttackData attackData, int element){
        //-1 lightning
        //0 wind
        //1 ice

        switch(element){
            case -1:
                if(lightningCharge >= elementChargeMax){
                    attackData.StatusEffect = PlayerStatusEffect.STUNNED;
                    attackData.StatusDuration = 1.3f;
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
                    attackData.StatusDuration = 5f;
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
                    attackData.StatusDuration = 5f;
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

    public void Nair(){
        attacks.SetMultiHitAttackID();
    }

    public void resetGravity(){
        movement.gravityPaused= false;
        rb.gravityScale = gravityScale;
    }

    public void dodgeRoll(){

        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.7f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
    }

    public void warpRoll(){
        facing = movement.Facing;
        rb.position += new Vector2(6f* facing,0f);
    }

    public void spawnStorm(){

        Vector3 pos = new Vector3(0f,6.5f,0f);
        GameObject MegurinStorm = Instantiate(entities.GetEntityPrefab("MegurinStorm"), transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in MegurinStorm.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID + 150;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }

        if(activeStorm){
            StartCoroutine(activeStorm.GetComponent<MegurinStorm>().Fade(0));
        }
        MegurinStorm.GetComponent<MegurinStorm>().attacks = attacks;
        activeStorm = MegurinStorm;

        entities.AddEntity(MegurinStorm);
    }

    public void spawnOrb(){

        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *4f,5,1f);
        GameObject MegurinOrb = Instantiate(entities.GetEntityPrefab("ChromaticOrb"), transform.position + pos, transform.rotation);
        MegurinOrb.transform.localScale = flip;
        MegurinOrb.GetComponent<Rigidbody2D>().velocity = new Vector2(facing *25, 0);
        MegurinOrb.GetComponent<Animator>().SetInteger("Mode",drifter.Charge);
        foreach (HitboxCollision hitbox in MegurinOrb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(MegurinOrb);
    }

    public void spawnSmallBolt(){

        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *10f,10f,1f);
        Vector3 pos = new Vector3(facing *3f,4,1f);
        GameObject smallBolt = Instantiate(entities.GetEntityPrefab("WeakBolt"), transform.position + pos, transform.rotation);
        smallBolt.transform.localScale = flip;
        foreach (HitboxCollision hitbox in smallBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(smallBolt);
    }
    public void spawnLargeBolt(){

        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *10f,10f,1f);
        Vector3 pos = new Vector3(facing *3f,4,1f);
        GameObject largeBolt = Instantiate(entities.GetEntityPrefab("StrongBolt"), transform.position + pos, transform.rotation);
        largeBolt.transform.localScale = flip;
        foreach (HitboxCollision hitbox in largeBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(largeBolt);
    }

    public void setLightning(){
        drifter.Charge = -1;
    }
    public void setIce(){
        drifter.Charge = 1;
    }
    public void setWind(){
        drifter.Charge = 0;
    }

    public void chargeNeutralW(){
        if(neutralWCharge < 8){
            neutralWCharge +=1;
        }
        else{
            sprite.color = new Color(1f,1f,.5f);
            drifter.SetAnimatorBool("Empowered",true);
        }
    }

    public void beginLightningbolt(){
        sprite.color = Color.white;
        if(anim.GetBool("Empowered") == true){
            drifter.SetAnimatorBool("Empowered",false);
            drifter.SetAnimatorBool("HasCharge",true);
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,1.4f);
        }
        else{
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.9f);
        }

    }
    public void fireLightningbolt(){
        neutralWCharge = 0;
        if(anim.GetBool("HasCharge") == true){
            spawnLargeBolt();
        }
        else{
            spawnSmallBolt();
        }

    }
    public void removeBoltStored(){
         drifter.SetAnimatorBool("HasCharge",false);
    }


    public override void cancelTheNeutralW()
    {
        rb.gravityScale = gravityScale;
        movement.gravityPaused= false;
    }
}
