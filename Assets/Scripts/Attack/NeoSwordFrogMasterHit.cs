using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{
    public int W_Down_Projectiles = 3;
    Tether tongueTether;
    Vector2 HeldDirection = Vector2.zero; 


    GameObject g_tongue;
    GameObject[] kunais = new GameObject[3];

    bool listeningForDirection = false;
    int delaytime = 0;
    int projnum;
       

    override public void UpdateFrame()
    {
        base.UpdateFrame();

        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            Empowered = false;
            drifter.Sparkle(false);
            projnum = 0;
        }

        if(movement.ledgeHanging || status.HasEnemyStunEffect())
        {
            if(g_tongue != null)deleteTongue();
            listeningForDirection = false;
            projnum = 0;
        }

        //Handle neutral special attacks
        if(listeningForDirection)
        {
            if(!drifter.input[0].Special) delaytime++;
            HeldDirection += new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
            if(HeldDirection != Vector2.zero || delaytime > 5) NeutralSpecialSlash();
        }

        if(projnum >0)
            fireKunaiGroundLine();
        else if(projnum <0)
            fireKunaiAirLine();

    }

    public void listenForDirection()
    {
        listeningForDirection = true;
        delaytime = 0;
    }

    public override void clearMasterhitVars()
    {
        base.clearMasterhitVars();
        listeningForDirection = false;
        projnum = 0;
        deleteTongue();
    }

    public new void returnToIdle()
    {
        base.returnToIdle();
        projnum = 0;
        deleteTongue();
    }

    public void NeutralSpecialSlash()
    {

        listeningForDirection = false;
        movement.updateFacing();
        
        if(HeldDirection.y <0 && movement.grounded) playState("W_Neutral_GD");
        else if(HeldDirection.y <0) playState("W_Neutral_D");
        else if(HeldDirection.y >0) playState("W_Neutral_U");
        else playState("W_Neutral_S");

        HeldDirection = Vector2.zero;

    }

     //Flips the direction the charactr is movement.Facing mid move)
    public void invertDirection()
    {
        movement.flipFacing();
    }

    //Grab Methods
    public void SpawnTongue()
    {
        if(g_tongue != null)deleteTongue();

        g_tongue = GameController.Instance.CreatePrefab("SF_Tongue", transform.position + new Vector3(2.3f * movement.Facing,1.6f), transform.rotation);
        g_tongue.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in g_tongue.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
        }
        g_tongue.transform.SetParent(drifter.gameObject.transform);
        g_tongue.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);

        tongueTether = g_tongue.GetComponentInChildren<Tether>();
        tongueTether.setTargetLength(.64f);
        tongueTether.setSpeed(4f);
    }

    public void deleteTongue()
    {
        if(g_tongue != null)Destroy(g_tongue);
        g_tongue = null;
    }

    public void setTongueLen(float len)
    {
        if(g_tongue == null)return;
        tongueTether.setTargetLength(len);
    }

    public void freezeTether()
    {
        if(g_tongue == null)return;
        tongueTether.freezeLen();
    }

    public void downSpecialProjectile()
    {
        projnum = W_Down_Projectiles;
    }

    public void downSpecialProjectileAir()
    {
        projnum = -1 * W_Down_Projectiles;
    }

    void fireKunaiGroundLine()
    {
        

        Vector3 size = new Vector3(10f * movement.Facing, 10f, 1f);
        Vector3 pos = new Vector3(.2f * movement.Facing, 2.7f, 1f);


        GameObject arrowA = GameController.Instance.CreatePrefab("Kunai", transform.position + new Vector3(1.5f * movement.Facing, 1.5f + W_Down_Projectiles/5f + (W_Down_Projectiles - projnum) * .6f, 0), transform.rotation);

        arrowA.transform.localScale = size;
        arrowA.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + 50f * movement.Facing, 0);

        foreach (HitboxCollision hitbox in arrowA.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;              
            hitbox.Facing = movement.Facing;
        }

        projnum--;

        refreshHitboxID();

    }


    void fireKunaiAirLine()
    {

        Vector3 size = new Vector3(10f, 10f, 1f);
        Vector3 pos = new Vector3(.2f * movement.Facing, 2.7f, 1f);

    
        float degreesA = movement.Facing >0 ? (335f  + projnum * 4f) : (215f  - projnum * 4f);
        float radiansA = degreesA * Mathf.PI/180f;
        float posDegrees = (movement.Facing >0 ? 335f  : 215f);
        float posRadians = posDegrees * Mathf.PI/180f;

        GameObject arrowA = GameController.Instance.CreatePrefab("Kunai", transform.position + new Vector3(movement.Facing * (-.5f - projnum/2f), projnum/-2f -.9f)
                                                                 + pos, 
                                                                 Quaternion.Euler(0,0,posDegrees));


        arrowA.transform.localScale = size;
        arrowA.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + (Mathf.Cos(posRadians) *50f), Mathf.Sin(posRadians)*50f);

        foreach (HitboxCollision hitbox in arrowA.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
        }

        projnum++;

        refreshHitboxID();
    }

    //Rollback
    //=========================================

    //Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
        MasterhitRollbackFrame baseFrame = SerializeBaseFrame();

        BasicProjectileRollbackFrame[] kunaiList = new BasicProjectileRollbackFrame[3];

        for(int i = 0; i < kunaiList.Length; i++)// (GameObject kunai in kunais)
            kunaiList[i] = kunais[i] != null ? kunais[i].GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null;

        baseFrame.CharacterFrame = new SwordfrogRollbackFrame() 
        {
            ListeningForDirection = listeningForDirection,
            Delaytime = delaytime,
            Projnum = projnum,
            Kunais = kunaiList,
            Tongue = g_tongue != null ? g_tongue.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,

        };

        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
        DeserializeBaseFrame(p_frame);
        SwordfrogRollbackFrame sf_frame = ((SwordfrogRollbackFrame)p_frame.CharacterFrame);

        listeningForDirection = sf_frame.ListeningForDirection;
        delaytime = sf_frame.Delaytime;
        projnum = sf_frame.Projnum;

        // for(int i = 0; i <3; i)


        // foreach(GameObject kunai in kunais)

    }

}

public class SwordfrogRollbackFrame: ICharacterRollbackFrame
{
    public string Type { get; set; }
    public bool ListeningForDirection;
    public int Delaytime;
    public int Projnum;

    public BasicProjectileRollbackFrame Tongue;

    public BasicProjectileRollbackFrame[] Kunais;
}
    