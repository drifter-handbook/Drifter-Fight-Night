using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MasterHit : MonoBehaviour, IMasterHit
{
    protected Drifter drifter;
    protected NetworkEntityList entities;
    // Start is called before the first frame update
    void Awake()
    {
        drifter = transform.parent.gameObject.GetComponent<Drifter>();
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void callTheAerial()
    {

    }
    public virtual void hitTheAerial(GameObject target)
    {

    }
    public virtual void cancelTheAerial()
    {

    }

    public virtual void callTheLight()
    {

    }
    public virtual void hitTheLight(GameObject target)
    {

    }
    public virtual void cancelTheLight()
    {

    }

    public virtual void callTheGrab()
    {

    }
    public virtual void hitTheGrab(GameObject target)
    {

    }
    public virtual void cancelTheGrab()
    {

    }

    public virtual void callTheRecovery()
    {

    }
    public virtual void hitTheRecovery(GameObject target)
    {

    }
    public virtual void cancelTheRecovery()
    {

    }

    //Side W
    public virtual void callTheSideW()
    {

    }
    public virtual void hitTheSideW(GameObject target)
    {

    }
    public virtual void cancelTheSideW()
    {

    }

    //Down W
    public virtual void callTheDownW()
    {

    }
    public virtual void hitTheDownW(GameObject target)
    {

    }
    public virtual void cancelTheDownW()
    {

    }

     //Neutral W
    public virtual void callTheNeutralW()
    {

    }
    public virtual void hitTheNeutralW(GameObject target)
    {

    }
    public virtual void cancelTheNeutralW()
    {

    }

     //Roll
    public virtual void callTheRoll()
    {

    }
    public virtual void hitTheRoll(GameObject target)
    {

    }
    public virtual void cancelTheRoll()
    {

    }
}
