using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrboHandler : StickToTarget
{

	public int color = 0;
	public GameObject owner;
	
	public ArrayList Orbos;
	public GameObject OrboPrefab;


	private PlayerStatus status;
	private PlayerAttacks attacks;
	private bool isHost = false;
	private NetworkHost host;

	public int orbToSpawn = 0;


	float rotation = 0;

	float radius = .5f;


    // Start is called before the first frame update
    void Start()
    {
        status = victim.GetComponent<PlayerStatus>();
        attacks = owner.GetComponent<PlayerAttacks>();
        host = GameController.Instance.host;
        isHost = GameController.Instance.IsHost;
        Orbos = new ArrayList();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(orbToSpawn > 0)
        	AddOrbo();


        //

        //rotate;

        for(int orboIndex = 0; orboIndex < Orbos.Count; orboIndex++ )
    	{
    		if(Orbos[orboIndex] == null)
    			Orbos.RemoveAt(orboIndex);
    		else
    			((GameObject)Orbos[orboIndex]).transform.localPosition = new Vector3(radius * Mathf.Cos(2f * Mathf.PI * (float)orboIndex / Orbos.Count),radius * Mathf.Sin(2f * Mathf.PI * (float)orboIndex / Orbos.Count),0);
    			
    	}

    	rotation += Time.deltaTime * 180f;

    	gameObject.transform.rotation =  Quaternion.Euler(0f,0f,rotation);

    	if(rotation >= 360) rotation = 0;


    	if(!status.HasStatusEffect(PlayerStatusEffect.ORBO) && radius > 0)
    		radius -= 1.5f *Time.deltaTime;

    	if(radius <= 0)
    		ClearOrbos();


    }


    void ClearOrbos()
    {
    	for(int orboIndex = 0; orboIndex < Orbos.Count; orboIndex++ )
    	{
    		((GameObject)Orbos[orboIndex]).GetComponent<SyncAnimatorStateHost>().SetState("OnHitState");
    		Orbos[orboIndex] = null;
    	}

    	Orbos = new ArrayList();
    }

    void AddOrbo()
    {
    	radius = .5f;
    	if(!isHost)return;
    	int target = Mathf.Min(Orbos.Count + orbToSpawn,5); 

    	for(int orboIndex = Orbos.Count; orboIndex < target; orboIndex++ )
    	{

    		UnityEngine.Debug.Log("ORBO SPAWNED");
    		GameObject orbo = host.CreateNetworkObject("Orbo", Vector3.zero, transform.rotation);
    		foreach (HitboxCollision hitbox in orbo.GetComponentsInChildren<HitboxCollision>(true))
        	{
            	hitbox.parent = owner;
            	hitbox.AttackID = attacks.AttackID + 120 + Orbos.Count;
            	hitbox.AttackType = attacks.AttackType;
            	hitbox.AttackData = attacks.Attacks[attacks.AttackType];
            	hitbox.Active = true;
            	hitbox.Facing = 1;
       		}
    		orbo.transform.SetParent(gameObject.transform , false);
    		orbo.GetComponent<SyncProjectileColorDataHost>().setColor(color);
    		Orbos.Add(orbo);  
    		orbo.transform.localPosition = new Vector3(.5f * Mathf.Cos(2 *Mathf.PI * (float)orboIndex / Orbos.Count),.5f * Mathf.Sin(2 * Mathf.PI * (float)orboIndex / Orbos.Count),0);
    		  		
    	}
    	orbToSpawn = 0;


    }
}
