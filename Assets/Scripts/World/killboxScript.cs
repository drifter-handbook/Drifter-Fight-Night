using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class killboxScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            // create death explosion
            NetworkEntityList Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
            GameObject deathExplosion = Instantiate(Entities.GetEntityPrefab("DeathExplosion"),
                other.transform.position,
                Quaternion.identity);
            // calculate pos
            Vector2 minPos = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f));
            Vector2 maxPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
            deathExplosion.transform.position = new Vector3(
                Mathf.Clamp(deathExplosion.transform.position.x, minPos.x, maxPos.x),
                Mathf.Clamp(deathExplosion.transform.position.y, minPos.y, maxPos.y),
                deathExplosion.transform.position.z
            );
            // calculate angle
            Vector2 center = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2));
            List<Vector2> bounds = new List<Vector2>()
            {
                new Vector3(Screen.width, Screen.height / 2),
                new Vector3(Screen.width, Screen.height),
                new Vector3(Screen.width / 2, Screen.height),
                new Vector3(0f, Screen.height),
                new Vector3(0f, Screen.height / 2),
                new Vector3(0f, 0f),
                new Vector3(Screen.width / 2, 0f),
                new Vector3(Screen.width, 0f)
            };
            float angle = Vector2.SignedAngle(Vector2.right, (Vector2)other.transform.position - center);
            // find the nearest octagonal angle
            float bestAngle = 0f;
            float min = float.MaxValue;
            for (int i = 0; i < bounds.Count; i++)
            {
                Vector2 pos = Camera.main.ScreenToWorldPoint(bounds[i]);
                float posAngle = Vector2.SignedAngle(Vector2.right, pos - center);
                float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(angle, posAngle));
                if (deltaAngle < min)
                {
                    bestAngle = posAngle;
                    min = deltaAngle;
                }
            }
            deathExplosion.transform.eulerAngles = new Vector3(0f, 0f, bestAngle);
            Entities.AddEntity(deathExplosion);

            // respawn
            if (other.gameObject != null)
            {
                other.gameObject.GetComponent<Drifter>().Stocks--;
                other.gameObject.GetComponent<Drifter>().DamageTaken = 0f;
            }
            if (Entities.hasStocks(other.gameObject))
            {
                other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                other.transform.position = new Vector2(0, 25);
                UnityEngine.Debug.Log("Stock Deducted");
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}
