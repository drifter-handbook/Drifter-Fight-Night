using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

// Handles the main menu screens before entering character select
public class OnlineMenuManager : UIMenuManager {

	public Sprite[] pips;
	Image[] objects = new Image[8];

	void Awake(){
		for(int i = 0; i <8; i++){
			objects[i] = GameObject.Find("P" + i).GetComponent<Image>();
		}
	}

	public void setPips(){
		for(int i = 0; i < 8; i++){
			if(GameController.Instance.controls.ContainsKey(i)){
				objects[i].sprite = (GameController.Instance.controls[i].Ready? pips[2]:pips[1]);
			}
			else
				objects[i].sprite = pips[0];
		}
	}
	
	void Start(){
		EventSystem.current.SetSelectedGameObject(GameObject.Find("Ready Up"));
	}

	public void ReturnToMainMenu() {
		GameController.Instance.GoToMainMenu();
	}

	public void ReadyUp(){
		GameController.Instance.ReadyUp();

	}
}