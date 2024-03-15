using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleScroll : MonoBehaviour {
    public RectTransform scrollRect;
    public RectTransform contentPanel;
    public UIMenuManager uiMenuManager;
    public UIMenuType targetMenu;
    public float topBuffer;
    public float bottomBuffer;

    private float viewportTopY;
    private float viewportBottomY;
    private Vector2 originalContentPanelYPos;

    private void Awake() {
        viewportBottomY = scrollRect.position.y + bottomBuffer;
        viewportTopY = scrollRect.position.y + (scrollRect.rect.height) + topBuffer;
        originalContentPanelYPos = contentPanel.anchoredPosition;
    }

    private void Update() {
        if(uiMenuManager.menuFlowHistory[uiMenuManager.menuFlowHistory.Count - 1] == targetMenu) {
            GameObject currentGameObject = EventSystem.current.currentSelectedGameObject;
            if(currentGameObject != null) {
                SnapTo();
            }
        }
    }

    public void SnapTo() {
        GameObject rect = EventSystem.current.currentSelectedGameObject;      
        bool inView = rect.transform.position.y > viewportBottomY && rect.transform.position.y < viewportTopY;

        if (!inView) { //If the selected Item is not visible.
            float buttonHeight = rect.GetComponent<RectTransform>().rect.height;

            if (rect.name == "Back Settings") { //scroll looped back to top.
                contentPanel.anchoredPosition = originalContentPanelYPos;    
            }
            else if (rect.transform.position.y < viewportBottomY) { //Out of range at top of the panel
                contentPanel.anchoredPosition += new Vector2(0, buttonHeight);
            }
            else if (rect.transform.position.y > viewportTopY) { //Out of range at bottom of the panel
                contentPanel.anchoredPosition += new Vector2(0, -buttonHeight);
            }
        }
    }
}
