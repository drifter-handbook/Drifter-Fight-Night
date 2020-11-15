using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{

   Camera self;
   float baseZoom;
   Vector3 basePos;
   bool killing = false;
   public Coroutine CurrentShake;

   void Awake(){
      self = GetComponent<Camera>();
      basePos = gameObject.transform.localPosition;
      baseZoom = self.orthographicSize;
   }

   public IEnumerator Shake(float duration, float magnitude)
   {
   		Vector3 origPos = transform.localPosition;
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

   		transform.localPosition = basePos;

   }

   public IEnumerator KillZoom(float duration, Vector3 position)
   {
      if(killing){
         yield break;
      }
      else{
         killing = true;
      }
      if(CurrentShake != null)StopCoroutine(CurrentShake);

      Vector3 origPos = transform.localPosition;

      UnityEngine.Debug.Log(position);
      UnityEngine.Debug.Log(transform.localPosition);

      //transform.localPosition = position;
      
      for(float i = 0f; i <= 1f;i+=.2f)
      {
            transform.localPosition = Vector3.Lerp(transform.localPosition,position,i);
            self.orthographicSize = Mathf.Lerp(self.orthographicSize,11f,i);
            yield return null;
      }
      GetComponentInChildren<SpriteRenderer>().enabled = true;
      CurrentShake = StartCoroutine(Shake(duration,.15f));
      yield return new WaitForSeconds(duration);
      GetComponentInChildren<SpriteRenderer>().enabled = false;  
      transform.localPosition = origPos;
      for(float i = 0f; i <= 1f;i+=.2f)
         {
            transform.localPosition = Vector3.Lerp(transform.localPosition,origPos,i);
            self.orthographicSize = Mathf.Lerp(self.orthographicSize,baseZoom,i);
            yield return null;
         }
      transform.localPosition = basePos;
      self.orthographicSize = baseZoom;
      killing = false;

      transform.localPosition = origPos;
      
   }
}
