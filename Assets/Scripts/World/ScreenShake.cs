using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScreenShake : MonoBehaviour
{

	Camera self;
	float baseZoom;
	Vector3 origPos;
	bool killing = false;
	bool isHost;
	bool DynamicCamera;


	Coroutine CurrentShake;
	Coroutine CurrentDarken;

	int zoomDurr = 0;
	int shakeDurr = 0;
	int darkenDurr = 0;
	float magnitude = 0f;

	public Drifter[] drifters;
	List<GameObject> paralaxLayers;

	void Awake() {
		isHost = GameController.Instance.IsHost;
		self = GetComponent<Camera>();
		origPos = gameObject.transform.localPosition;
		baseZoom = self.orthographicSize;
		DynamicCamera = PlayerPrefs.GetInt("dynamicCamera") != 0;
	}

	public void getParalax() {
		//Populate a list of paralax layers from the instantiated stage
		paralaxLayers = new List<GameObject>();
		for(int i = 0; i <5; i++)
			paralaxLayers.Add(GameObject.Find("Paralax_" + i));
	}

	void FixedUpdate() {

		if(drifters == null || !DynamicCamera || killing) return;
		//Get cneterpoint once per frame to save on performance.
		Vector3 centerpoint = CalculateCenter();

		if(CurrentShake == null) 
		{
			transform.localPosition = Vector3.Lerp(centerpoint,transform.localPosition,Time.deltaTime/1.5f);
			for(int i = 0; i < paralaxLayers.Count; i++)	{
				//If the paralax layer at index i exists, adjust its position accordingly
				if(paralaxLayers[i] != null)
					paralaxLayers[i].transform.localPosition = Vector3.Lerp(centerpoint/(7.5f-1.5f*i),transform.localPosition,Time.deltaTime/1.5f);
			}
		}

		if(!killing) self.orthographicSize = CalculateZoom();

		UpdateDarken();
		UpdateShake();

	}

	public void Shake(int p_duration, float p_magnitude) {
		shakeDurr = p_duration;
		magnitude = p_magnitude;
	}

	public void Darken(int p_duration) {
		if(killing)return;
		darkenDurr = p_duration;
		GetComponentInChildren<SyncAnimatorStateHost>().SetState("Darken"); 
	}

	void UpdateShake() {
			if(shakeDurr > 0)	{
				Vector3 origPos = (killing||!DynamicCamera)?transform.localPosition:CalculateCenter();
		
				transform.localPosition = origPos;
				float x = origPos.x + Random.Range(-1f,1f) * magnitude * self.orthographicSize/15f;
				float y = origPos.y + Random.Range(-.5f,.5f) * magnitude * self.orthographicSize/15f;

				transform.localPosition = new Vector3(x,y,origPos.z);
				// self.orthographicSize += Random.Range(-2f,2f) * magnitude;
				shakeDurr--;
				if(shakeDurr <=0)
					transform.localPosition = origPos;

				//if(!DynamicCamera) transform.localPosition = origPos;
			}
			

	}

	void UpdateDarken() {
		if(darkenDurr > -50)	{
			int prevDurr = darkenDurr;
			darkenDurr--;
			if(darkenDurr <=0 && prevDurr >0)
				GetComponentInChildren<SyncAnimatorStateHost>().SetState("Lighten");
			else if(darkenDurr <=-50)
				GetComponentInChildren<SyncAnimatorStateHost>().SetState("Hidden");
		}
		
	}



	private Vector3 CalculateCenter() {
		Vector2 centerpoint = Vector2.zero;
		for(int i = 0; i < drifters.Length; i++)	{
			//If a player has died, remove them from the list for future iterations
			if(drifters[i] == null)drifters = drifters.Where(val => val != null).ToArray();

			Vector2 currPos = drifters[i].gameObject.GetComponent<Rigidbody2D>().position;
			centerpoint += new Vector2(Mathf.Clamp(currPos.x,-10f,10f),Mathf.Clamp(currPos.y,-10f,10f));

		}

		return centerpoint/(drifters.Length +1);
	}

	private float CalculateZoom() {
		float scaledZoom = 0;
		Vector2 centerpoint = transform.localPosition;
		for(int i = 0; i < drifters.Length; i++)	{
			//If a player has died, remove them from the list for future iterations
			try{

				if(drifters[i] == null)drifters = drifters.Where(val => val != null).ToArray();

				Vector2 currPos = drifters[i].gameObject.GetComponent<Rigidbody2D>().position;
				scaledZoom = Mathf.Max(Vector2.Distance(new Vector2(Mathf.Clamp(currPos.x,-20f,20f),Mathf.Clamp(currPos.y,-10f,30f)),centerpoint),scaledZoom);
			}
			catch(System.IndexOutOfRangeException)	{

			  return 30;
			}


		}

		return Mathf.Lerp(self.orthographicSize,Mathf.Clamp(scaledZoom *1.7f,15f,30f),Time.deltaTime * 3f);
	}

	public void zoomEffect(int p_duration, Vector3 p_position, bool p_finalKill) {
		// if(killing) return;
		
		// else
		//    killing = true;

		// //Stops the current screenshake process, if one is active
		// shakeDurr = 0;

		// zoomDurr = p_duration;

		// GetComponentInChildren<SyncAnimatorStateHost>().SetState(finalKill?"Final_Kill":"Critical_Attack"); 
		UnityEngine.Debug.Log("Reimplement Me");
	}

	public void UpdateZoom() {

		// //Enables the zoom background
		

		// //Zoom in
		// for(float i = 0f; i <= 1f;i+=.05f)
		// {
		//       transform.localPosition = Vector3.Lerp(transform.localPosition,position,i);
		//       self.orthographicSize = Mathf.Lerp(self.orthographicSize,13f,i);
		//       yield return null;
		// }
		
		// Shake(duration,.9f);
		// yield return new WaitForSeconds(duration);
		// GetComponentInChildren<SyncAnimatorStateHost>().SetState("Hidden"); 
		 
		// //transform.localPosition = origPos;

		// //Zoom Out

		// for(float i = 0f; i <= 1f;i+=(DynamicCamera ? .5f : .01f))
		// {
		//       transform.localPosition = Vector3.Lerp(transform.localPosition,DynamicCamera?CalculateCenter():origPos,i);
		//       self.orthographicSize = Mathf.Lerp(self.orthographicSize,DynamicCamera?CalculateZoom():baseZoom,i);
		//       yield return null;
		// }

		// if(!DynamicCamera)
		// {
		//    transform.localPosition = origPos;
		//    self.orthographicSize = baseZoom;
		// }

		// killing = false;
		
	}

	// public IEnumerator darkenScreen(float duration)
	// {
	//    if(killing){
	//       yield break;
	//    }
	//    GetComponentInChildren<SyncAnimatorStateHost>().SetState("Darken"); 
	//    yield return new WaitForSeconds(duration);
	//    GetComponentInChildren<SyncAnimatorStateHost>().SetState("Lighten");
	//    yield return new WaitForSeconds(.84f);
	//    GetComponentInChildren<SyncAnimatorStateHost>().SetState("Hidden");
	//    yield break;
	// }

}
