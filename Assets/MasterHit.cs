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
        attacking = parent.GetComponent<PlayerAttacking>();
        playerMovement = parent.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        attacking = parent.GetComponent<PlayerAttacking>();
        playerMovement = parent.GetComponent<PlayerMovement>();
    }
    public void callTheAttackEffect(){
        //call the Attack here (using the parent)
    }
    public void callTheMovementEffect(){
        //call the movement caused by the attack here (in order to force move)
    }
}
