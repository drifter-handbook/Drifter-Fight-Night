using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird_Wrangler : MonoBehaviour
{

	PlayerAttacks attacks;
    GameObject Mytharius;
    int facing;
    int color;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
     	rb = GetComponent<Rigidbody2D>();   
    }

    public void setup(GameObject p_drifter, PlayerAttacks p_attacks, int p_facing,int p_color)
    {
    	Mytharius = p_drifter;
    	attacks = p_attacks;
    	facing = p_facing;
    	color = p_color;
    }

    public void Drop_Letter()
    {
        if(!GameController.Instance.IsHost)return;

        GameObject letter = GameController.Instance.host.CreateNetworkObject("Mytharius_Letter", transform.position, transform.rotation);
        letter.transform.localScale = new Vector3(facing *10,10,1f);
        attacks.SetMultiHitAttackID();
        foreach (HitboxCollision hitbox in letter.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = Mytharius;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = facing;
        }

        letter.GetComponent<SyncProjectileColorDataHost>().setColor(color);
    }

    public void remove_Gravity()
    {
    	rb.gravityScale = 0;
    }

}
