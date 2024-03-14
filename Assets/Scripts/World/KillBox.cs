using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour {
	public ScreenShake Shake;

	void Awake() {
		foreach(CharacterSelectState css in CharacterMenu.charSelStates)
			if(css != null) css.GameStandings = 1;
	}

	GameObject CreateExplosion(Collider2D other, int playerIndex) {
		GameObject deathExplosion = GameController.Instance.CreatePrefab("DeathExplosion", other.transform.position, Quaternion.identity);
		deathExplosion.transform.position =
			ClampObjectToScreenSpace.FindPosition(deathExplosion.transform);
		deathExplosion.transform.eulerAngles = new Vector3(0,0,((other.gameObject.GetComponent<Rigidbody2D>().velocity.y>0)?1:-1) * Vector3.Angle(other.gameObject.GetComponent<Rigidbody2D>().velocity, new Vector3(1f,0,0)));
			//ClampObjectToScreenSpace.FindNearestOctagonalAngle(deathExplosion.transform);
		return deathExplosion;    
	}

	int calculateGameStandings(){
		int standing = 0;
		foreach(CharacterSelectState css in CharacterMenu.charSelStates){
			if(css != null){
				UnityEngine.Debug.Log(css);
				if(css.GameStandings == 1) standing++;
			}
		}
		UnityEngine.Debug.Log(standing);
		//End the game if one of the last two players died
		if(standing <= 2) GameController.Instance.EndMatch();
		return standing;
	}

	void OnTriggerExit2D(Collider2D other) {
		killPlayer(other);
	}


	protected void killPlayer(Collider2D other) {

		if (other.gameObject.tag == "Player" && other.GetType() == typeof(BoxCollider2D))
		{
			while(Shake==null)Shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();
			Drifter drifter = other.gameObject?.GetComponent<Drifter>();
			if (!drifter.status.HasStatusEffect(PlayerStatusEffect.DEAD) && !drifter.status.HasStatusEffect(PlayerStatusEffect.BANISHED)) {   

				Shake?.Shake(18, 1.5f);
				Shake?.Darken(6);
				CreateExplosion(other, -1);
				drifter.die();

				if (drifter.Stocks <= 0) {
					drifter.status.ApplyStatusEffect(PlayerStatusEffect.BANISHED, 999);
					CharacterMenu.charSelStates[drifter.peerID].GameStandings = calculateGameStandings();    
				}
			}
		}
	}
}