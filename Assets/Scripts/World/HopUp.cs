using UnityEngine;

public class HopUp : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
        	UnityEngine.Debug.Log("CORNER HIT");

        	col.gameObject.GetComponent<PlayerMovement>().currentJumps++;
            // other.transform.position = new Vector3(transform.position.x,
            //     transform.position.y + 8.0f, 1);

            // other.rigidbody.velocity = Vector3.zero;
        }
    }
}