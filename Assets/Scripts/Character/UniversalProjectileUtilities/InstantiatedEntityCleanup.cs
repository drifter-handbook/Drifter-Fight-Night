using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatedEntityCleanup : MonoBehaviour
{
	public float duration = -1;

	void Start()
	{
		if(duration >0)
		{
			StartCoroutine(decay());
		}
	}


	IEnumerator decay(){
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    public void Cleanup()
    {
        Destroy(gameObject);
    }
}
