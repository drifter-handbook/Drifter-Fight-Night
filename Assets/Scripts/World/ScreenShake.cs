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

   public Drifter[] drifters;

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


   void Update()
   {
      if(drifters == null || PlayerPrefs.GetInt("dynamicCamera") == 0)return;

      Vector2 centerpoint = Vector2.zero;
      float scaledZoom = 0;


      foreach(Drifter drifter in drifters)
      {
         Vector2 currPos = drifter.gameObject.GetComponent<Rigidbody2D>().position;
         centerpoint += new Vector2(Mathf.Clamp(currPos.x,-10f,10f),Mathf.Clamp(currPos.y,-10f,10f));
      }

      centerpoint = centerpoint/(drifters.Length +1);

      foreach(Drifter drifter in drifters)
      {
         Vector2 currPos = drifter.gameObject.GetComponent<Rigidbody2D>().position;
         scaledZoom =  Mathf.Max(Vector2.Distance(new Vector2(Mathf.Clamp(currPos.x,-20f,20f),Mathf.Clamp(currPos.y,-10f,30f)),centerpoint),scaledZoom);
      }

      if(CurrentShake == null) transform.localPosition = Vector3.Lerp(centerpoint,transform.localPosition,Time.deltaTime/1.5f);

      if(!killing) self.orthographicSize = Mathf.Lerp(self.orthographicSize,Mathf.Clamp(scaledZoom *1.7f,20f,30f),Time.deltaTime * 3f);

   }

   public void statShakeCoroutine(float duration, float magnitude)
   {
      if(!isHost || killing)return;
      if(CurrentShake != null) StopCoroutine(CurrentShake);
      CurrentShake = StartCoroutine(Shake(duration,magnitude));

   }


   IEnumerator Shake(float duration, float magnitude)
   {
         if(!isHost)yield break;
   		Vector3 origPos = transform.localPosition;
   		float elapsed = 0f;

   		while(elapsed < duration)
   		{
   			transform.localPosition = origPos;
   			float x = origPos.x + Random.Range(-1f,1f) * magnitude * (self.orthographicSize - 15f)/15f;
   			float y = origPos.y + Random.Range(-.5f,.5f) * magnitude * (self.orthographicSize - 15f)/15f;

   			transform.localPosition = new Vector3(x,y,origPos.z);

            // self.orthographicSize += Random.Range(-2f,2f) * magnitude;

   			elapsed += Time.deltaTime;

   			yield return null;

   		}

   		//transform.localPosition = origPos;

         CurrentShake = null;

         yield break;

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
