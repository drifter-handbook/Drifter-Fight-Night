using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeanWrangler : MonoBehaviour
{
    // Start is called before the first frame update
    public bool hide;
    public Animator anim;
    public PlayerAttacks attacks;
    Rigidbody2D rb;

    void Start()
    {
        
    }

    public void turn(int facing){
        transform.localScale = new Vector3(transform.localScale.x * facing,transform.localScale.y,transform.localScale.z);
    }

    IEnumerator delete(){
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
        yield break;
    }

    public void setAttackId(){
        attacks.SetMultiHitAttackID();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
