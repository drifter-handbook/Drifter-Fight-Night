using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectableProjectile : MonoBehaviour
{

	protected Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        if(!GameController.Instance.IsHost)return;
        if(col.gameObject.name == "Reflector"){
            rb.velocity =  new Vector2(rb.velocity.x * -1.5f,rb.velocity.y);

            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1,gameObject.transform.localScale.y,gameObject.transform.localScale.z);

            GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.REFLECT, Vector3.Lerp(col.gameObject.transform.position, transform.position, 0.1f), 0, new Vector2(10f, 10f));

            foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = col.gameObject.transform.parent.GetComponentInChildren<HitboxCollision>().parent;
                    hitbox.Facing*=-1;
                    //Mkae this not suck laters
                    hitbox.AttackID = 300 + Random.Range(0,25);
                }
        }
    }
}
