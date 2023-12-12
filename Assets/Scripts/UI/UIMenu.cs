using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIMenuType
{
    MainMenu,
    ModeMenu,
    LocalMenu,
    OnlineMenu,
    HostMenu,
    JoinMenu,
    SettingsMenu
}

public class UIMenu : MonoBehaviour
{
    [SerializeField]
    public UIMenuType currentMenu;
}
