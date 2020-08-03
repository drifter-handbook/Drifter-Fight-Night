using UnityEngine;

public class HopUp : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other)
    {
        UnityEngine.Debug.Log("Collision");
        if (other.gameObject.tag == "Player")
        {
            other.transform.position = new Vector3(transform.position.x,
                transform.position.y + 8.0f, 1);


            other.rigidbody.velocity = Vector3.zero;
            UnityEngine.Debug.Log("Warped");
        }
    }
}