using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    // Start is called before the first frame update

    public PlayerStatus status;
    public Animator anim;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // if(anim.GetBool("Walking")){
        //     GameObject SmokeTrail = Instantiate(entities.GetEntityPrefab("SmokeTrail"), transform.position, transform.rotation);
        //     SmokeTrail.transform.localScale = new Vector3(100f,100f,1f);
        //     entities.AddEntity(SmokeTrail);
        // }
    }

}
