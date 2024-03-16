using System.Collections.Generic;
using UnityEngine;

public enum UIMenuType {
    MainMenu,
    ModeMenu,
    LocalMenu,
    OnlineMenu,
    HostMenu,
    JoinMenu,
    SettingsMenu,
    RebindMenu,
    InGamePauseMenu,
    InGameSettingsMenu,
    InGameTrainingOptionsMenu,
    Invalid
}

public class UIMenu : MonoBehaviour {
    [SerializeField]
    public UIMenuType currentMenu;
    [SerializeField]
    public List<GameObject> gameObjects;
}
