using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectShake : MonoBehaviour
{

   Vector3 origPos;

   void Awake(){
     origPos = transform.localPosition;
   }

   public IEnumerator Shake(float duration, float magnitude)
   {
   		
   		float elapsed = 0f;


   		while(elapsed < duration)
   		{
   			//transform.localPosition = origPos;
   			float x = origPos.x + Random.Range(-1f,1f) * magnitude;
   			float y = origPos.y + Random.Range(-.5f,.5f) * magnitude;

   			transform.localPosition = new Vector3(x,y,origPos.z);

   			elapsed += Time.deltaTime;

   			yield return null;

   		}

   		transform.localPosition = origPos;

   }
}
