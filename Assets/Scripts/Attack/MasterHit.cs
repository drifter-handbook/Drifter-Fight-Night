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
}
