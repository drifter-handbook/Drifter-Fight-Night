using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherRange : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rb;
    public Vector3 TetherPoint;
    public Vector3 enemyVelocity;
    public string tag;


    void Start()
    {

    }


    IEnumerator Delete(){
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
        yield break;
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if(col.gameObject.tag == tag)
        {
            if(tag == "Player")enemyVelocity = col.gameObject.GetComponent<Rigidbody2D>().velocity;
            TetherPoint = col.gameObject.transform.position;
        }
    }

    void OnTriggerExit2D(Collider2D col){
        if(col.gameObject.tag == tag)
        {
            TetherPoint = Vector3.zero;
            enemyVelocity = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
