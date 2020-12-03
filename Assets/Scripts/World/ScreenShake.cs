using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour , INetworkInit
{

   Camera self;
   float baseZoom;
   Vector3 basePos;
   bool killing = false;
   bool isHost;


   public Coroutine CurrentShake;

   void Awake()
   {
      isHost = GameController.Instance.IsHost;
      self = GetComponent<Camera>();
      basePos = gameObject.transform.localPosition;
      baseZoom = self.orthographicSize;
   }

   public void OnNetworkInit()
   {
        NetworkUtils.RegisterChildObject("Dynamic_Background", transform.Find("Dynamic_Background").gameObject);
   }

   public IEnumerator Shake(float duration, float magnitude)
   {
         if(!isHost)yield break;
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

   public IEnumerator zoomEffect(float duration, Vector3 position, bool finalKill)
   {
      if(killing || !isHost){
         yield break;
      }
      else{
         killing = true;
      }
      if(CurrentShake != null)StopCoroutine(CurrentShake);

      Vector3 origPos = transform.localPosition;

      //transform.localPosition = position;
      
      GetComponentInChildren<SyncAnimatorStateHost>().SetState(finalKill?"Final_Kill":"Critical_Attack"); 
      for(float i = 0f; i <= 1f;i+=.05f)
      {
            transform.localPosition = Vector3.Lerp(transform.localPosition,position,i);
            self.orthographicSize = Mathf.Lerp(self.orthographicSize,13f,i);
            yield return null;
      }
      
      CurrentShake = StartCoroutine(Shake(duration,.2f));
      yield return new WaitForSeconds(duration);
      GetComponentInChildren<SyncAnimatorStateHost>().SetState("Hidden"); 
       
      transform.localPosition = origPos;
      for(float i = 0f; i <= 1f;i+=.01f)
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
