using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoBojoMasterHit : MasterHit
{

    GameObject centaur;
    GameObject soundwave;
    int power = 0;


    //Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
        MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
        DeserializeBaseFrame(p_frame);
    }

    public void SpawnDownTiltWave()
    {
        
        //Vector3 pos = new Vector3(2f * movement.Facing,2.7f,0);
        
        GameObject wave = GameController.Instance.CreatePrefab("BojoDTiltWave", transform.position , transform.rotation);
        wave.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

        wave.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 33,-22);
        foreach (HitboxCollision hitbox in wave.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = -attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Facing = movement.Facing;
       }

       wave.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
    }

    public void whirl()
    {

        movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Bojo_Whirl, false);
    }

    public void Neutral_Special()
    {

        
        if(soundwave!= null) 
        {
                //Detonate here;
            return;
        }
        soundwave = GameController.Instance.CreatePrefab("Bojo_Note", transform.position + new Vector3(1.5f * movement.Facing, 4f), transform.rotation);
        soundwave.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in soundwave.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            
            hitbox.Facing = movement.Facing;
        }
        soundwave.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
        soundwave.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 18f,0,0);
    }


    public void SpawnCentaur()
    {

        if(centaur == null)
        {
            
                //Vector3 pos = new Vector3(2f * movement.Facing,2.7f,0);
        
            centaur = GameController.Instance.CreatePrefab("Centaur", transform.position , transform.rotation);
            centaur.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);


            centaur.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 15,0);
            foreach (HitboxCollision hitbox in centaur.GetComponentsInChildren<HitboxCollision>(true))
            {
                    hitbox.parent = drifter.gameObject;
                    hitbox.AttackID = -attacks.AttackID;
                    hitbox.AttackType = attacks.AttackType;
                    hitbox.Facing = movement.Facing;
            }

            foreach (HurtboxCollision hurtbox in centaur.GetComponentsInChildren<HurtboxCollision>(true))
                hurtbox.owner = drifter.gameObject;
       

            centaur.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
            UnityEngine.Debug.Log("PLACING CENTAUR");
            centaur.GetComponent<SyncAnimatorStateHost>().SetState("Centaur_" + power);
            UnityEngine.Debug.Log("Centaur_" + power);
        }
    }

    public void fireCentaur()
    {
        if(centaur != null)
        {
            centaur.GetComponent<SyncAnimatorStateHost>().SetState("Centaur_Fire_" + power);
            UnityEngine.Debug.Log("Centaur_Fire_" + power);
            
        }
        power = 0;
    }

    public void fireCentaurState()
    {
        if(centaur != null)
            playState("W_Down_Fire");
    }

    public void setCentaurPower(int pow)
    {
        power = pow; 
        
    }
}
