using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectShake : MonoBehaviour
{

   Vector3 origPos;

   int duration = 0;
   float magnitude = 0;

   void Awake(){
     origPos = transform.localPosition;
   }

   void FixedUpdate()
   {
      UpdateShake();
   }

   public void Shake(int p_duration, float p_magnitude)
   {
      duration = p_duration;
      magnitude = p_magnitude;
   }

   void UpdateShake()
   {
      if(duration > 0)
      {
         float x = origPos.x + Random.Range(-1f,1f) * magnitude;
         float y = origPos.y + Random.Range(-.5f,.5f) * magnitude;

         transform.localPosition = new Vector3(x,y,origPos.z);
         duration--;
         if(duration <=0)
            transform.localPosition = origPos;
      }
   }
}
