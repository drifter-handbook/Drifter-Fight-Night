using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeGrab : MonoBehaviour
{

    public float duration;
    public Drifter drifter;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Fade(duration));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.name == "Hurtboxes"){
            drifter.SetAnimatorTrigger("GrabbedPlayer");
        }
        
    }

    public IEnumerator Fade(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject.transform.parent.gameObject);
        yield break;
    }
}
