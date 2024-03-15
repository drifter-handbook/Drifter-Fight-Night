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

    //This scroll functionality only works for vertical lists at the moment. Will need to add horizontal support if need arises.

    private void Awake() {
        viewportBottomY = scrollRect.position.y + bottomBuffer;
        viewportTopY = scrollRect.position.y + (scrollRect.rect.height) + topBuffer;
        originalContentPanelYPos = contentPanel.anchoredPosition;
    }

    private void Update() {
        if(uiMenuManager.menuFlowHistory[uiMenuManager.menuFlowHistory.Count - 1] == targetMenu) {
            GameObject currentGameObject = EventSystem.current.currentSelectedGameObject;
            if(currentGameObject != null) {
                TargetOutOfViewHighlightedItem();
            }
        }
    }

    public void TargetOutOfViewHighlightedItem() {
        GameObject rect = EventSystem.current.currentSelectedGameObject;      
        bool inView = rect.transform.position.y > viewportBottomY && rect.transform.position.y < viewportTopY;

        if (!inView) { //If the selected Item is not visible.
            float buttonHeight = rect.GetComponent<RectTransform>().rect.height;

            if (rect.transform.position.y < viewportBottomY) { //Highlighted button is out of view at bottom of the panel
                contentPanel.anchoredPosition += new Vector2(0, buttonHeight);
            }
            else if (rect.transform.position.y > viewportTopY) { //Highlighted button is of view at top of the panel
                contentPanel.anchoredPosition += new Vector2(0, -buttonHeight);
            }
        }
    }
}
