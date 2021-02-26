using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScreenShake : MonoBehaviour , INetworkInit
{

   Camera self;
   float baseZoom;
   Vector3 basePos;
   bool killing = false;
   bool isHost;
   bool DynamicCamera;


   public Coroutine CurrentShake;

   public Drifter[] drifters;

   void Awake()
   {
      isHost = GameController.Instance.IsHost;
      self = GetComponent<Camera>();
      basePos = gameObject.transform.localPosition;
      baseZoom = self.orthographicSize;
      DynamicCamera = PlayerPrefs.GetInt("dynamicCamera") != 0;
   }

   public void OnNetworkInit()
   {
        NetworkUtils.RegisterChildObject("Dynamic_Background", transform.Find("Dynamic_Background").gameObject);
   }


   void Update()
   {
      if(drifters == null || !DynamicCamera || killing) return;

      if(CurrentShake == null) transform.localPosition = Vector3.Lerp(CalculateCenter(),transform.localPosition,Time.deltaTime/1.5f);

      if(!killing) self.orthographicSize = CalculateZoom();

   }

   public void startShakeCoroutine(float duration, float magnitude)
   {
      if(!isHost || killing)return;
      if(CurrentShake != null) StopCoroutine(CurrentShake);
      CurrentShake = StartCoroutine(Shake(duration,magnitude));

   }

   static bool isNotNull(Object n)
   {
      return n != null;
   }

   IEnumerator Shake(float duration, float magnitude)
   {
         if(!isHost)yield break;
   		Vector3 origPos = (killing||!DynamicCamera)?transform.localPosition:CalculateCenter();
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

   		if(!DynamicCamera) transform.localPosition = basePos;

         CurrentShake = null;

         yield break;

   }

   private Vector3 CalculateCenter()
   {
      Vector2 centerpoint = Vector2.zero;
      for(int i = 0; i < drifters.Length; i++)
      {
         //If a player has died, remove them from the list for future iterations
         if(drifters[i] == null)drifters = drifters.Where(val => val != null).ToArray();

         Vector2 currPos = drifters[i].gameObject.GetComponent<Rigidbody2D>().position;
         centerpoint += new Vector2(Mathf.Clamp(currPos.x,-10f,10f),Mathf.Clamp(currPos.y,-10f,10f));

      }

      return centerpoint/(drifters.Length +1);
   }

   private float CalculateZoom()
   {
      float scaledZoom = 0;
      Vector2 centerpoint = transform.localPosition;
      for(int i = 0; i < drifters.Length; i++)
      {
         //If a player has died, remove them from the list for future iterations
         if(drifters[i] == null)drifters = drifters.Where(val => val != null).ToArray();

         Vector2 currPos = drifters[i].gameObject.GetComponent<Rigidbody2D>().position;
         scaledZoom = Mathf.Max(Vector2.Distance(new Vector2(Mathf.Clamp(currPos.x,-20f,20f),Mathf.Clamp(currPos.y,-10f,30f)),centerpoint),scaledZoom);

      }

      return Mathf.Lerp(self.orthographicSize,Mathf.Clamp(scaledZoom *1.7f,20f,30f),Time.deltaTime * 3f);
   }


   public IEnumerator zoomEffect(float duration, Vector3 position, bool finalKill)
   {
      //Killing is a flag that indicates if a zoom effect is happening
      //This disallows many other screen effects from occuring that may cause zooms to jank out
      if(killing || !isHost){
         yield break;
      }
      else{
         killing = true;
      }

      //Stops the current screenshake process, if one is active
      if(CurrentShake != null)StopCoroutine(CurrentShake);

      //Saves Starting position
      //Vector3 origPos = transform.localPosition;

      //Enables the zoom background
      GetComponentInChildren<SyncAnimatorStateHost>().SetState(finalKill?"Final_Kill":"Critical_Attack"); 

      //Zoom in
      for(float i = 0f; i <= 1f;i+=.05f)
      {
            transform.localPosition = Vector3.Lerp(transform.localPosition,position,i);
            self.orthographicSize = Mathf.Lerp(self.orthographicSize,13f,i);
            yield return null;
      }
      
      CurrentShake = StartCoroutine(Shake(duration,.9f));
      yield return new WaitForSeconds(duration);
      GetComponentInChildren<SyncAnimatorStateHost>().SetState("Hidden"); 
       
      //transform.localPosition = origPos;

      //Zoom Out

      for(float i = 0f; i <= 1f;i+=(DynamicCamera ? .5f : .01f))
      {
            transform.localPosition = Vector3.Lerp(transform.localPosition,DynamicCamera?CalculateCenter():basePos,i);
            self.orthographicSize = Mathf.Lerp(self.orthographicSize,DynamicCamera?CalculateZoom():baseZoom,i);
            yield return null;
      }

      if(!DynamicCamera)
      {
         transform.localPosition = basePos;
         self.orthographicSize = baseZoom;
      }

      killing = false;
      
   }
}
