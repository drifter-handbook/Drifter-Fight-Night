using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

// Handles the main menu screens before entering character select
public class MainMenuScreensManager : UIMenuManager
{
	public GameObject savedIPObject;
	public GameObject roomNameObject;

	public Toggle toggle1;
	public Toggle toggle2;
	public Toggle toggle3;

	PlayerInput[] playerInputs;

	[SerializeField]
	public InputSystemUIInputModule uiInputModule;
	void Awake() {
		InitializeMenus();
	}
	public override void OnMenuActivated(UIMenuType type) {
		if (type == UIMenuType.SettingsMenu) {
			toggle1.onValueChanged.RemoveAllListeners();
			toggle2.onValueChanged.RemoveAllListeners();
			toggle3.onValueChanged.RemoveAllListeners();

			toggle1.isOn = PlayerPrefs.GetInt("dynamicCamera") > 0;
			toggle2.isOn = PlayerPrefs.GetInt("HidePing") > 0;
			toggle3.isOn = PlayerPrefs.GetInt("HideTextInput") > 0;
			//   ^ toggles the code too. Why? idk, unity makes interesting decisions sometimes

			toggle2.onValueChanged.AddListener(delegate {
				togglePing();
			});
		}

		base.OnMenuActivated(type);
	}

	void FixedUpdate() {
		playerInputs = FindObjectsOfType<PlayerInput>();
		foreach(PlayerInput playerInput in playerInputs) {
			if (playerInput != null && playerInput.currentActionMap.FindAction("Cancel").triggered) {
				if (activeMenu == UIMenuType.MainMenu) {
					Application.Quit();
					return;
				}
				else {
					Debug.Log("Pressed back time to die: " + menuFlowHistory[menuFlowHistory.Count - 1] + "\n");
					ReturnToPriorMenu();
					return;
				}
			}
			UpdateActivePlayerInputs(playerInput);
		}
	}

	public void togglePing() {
		if (PlayerPrefs.GetInt("HidePing") == 0) { PlayerPrefs.SetInt("HidePing", 1); }
		else { PlayerPrefs.SetInt("HidePing", 0); }
		PlayerPrefs.Save();
	}

	public void saveRoomCode() {
		PlayerPrefs.SetString("savedIP",savedIPObject.GetComponent<InputField>().text);
	}

	//0 = online
	//1 = Local
	//2 = Training
	//3 = story?

	// void FixedUpdate(){
	//     if(startcd > 0){
	//         startcd--;
	//         if(startcd ==0)
	//              StartGame(0);
	//     }
	// }


	//-------------------------------------------------------------
	// ONLINE MENU MECHANCIS
	//-------------------------------------------------------------

	public void StartHost() {
		GameController.Instance.StartHost();
	}

	public void StopHost() {
		GameController.Instance.StopHost();
	}

	public void StartClient() {
		GameController.Instance.StopClient();
	}

	public void StopClient() {
		GameController.Instance.StartHost();
	}

	public void StartGame(int mode = 0){
		GameController.Instance.StartGame(mode);
	}

	public void toggleDynamicCamera() {
		PlayerPrefs.SetInt("dynamicCamera",toggle1.isOn?1:0);
	}
  
}
