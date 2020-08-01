using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterHit : MonoBehaviour, IMasterHit
{
    PlayerAttacking attacking;
    PlayerMovement playerMovement;
    public GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void callTheAerial()
    {

    }
    public void cancelTheAerial()
    {

    }
    public void callTheLight()
    {

    }
    public void cancelTheLight()
    {

    }
    public void callTheGrab()
    {

    }
    public void cancelTheGrab()
    {

    }
    public void callTheRecovery()
    {

    }
    public void cancelTheRecovery()
    {

    }
    public void callTheAttackEffect()
    {
        //call the Attack here (using the parent)
    }
    public void callTheMovementEffect()
    {
        //call the movement caused by the attack here (in order to force move)
    }
}
