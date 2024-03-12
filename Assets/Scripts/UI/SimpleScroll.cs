using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleScroll : MonoBehaviour {
    public List<GameObject> listItems;
    public RectTransform scrollRect;
    public RectTransform contentPanel;
    public UIMenuManager uiMenuManager;

    private RectTransform oldRect;
    void Update() {
        if(uiMenuManager.menuFlowHistory[uiMenuManager.menuFlowHistory.Count - 1] == UIMenuType.RebindMenu) {
            GameObject currentGameObject = EventSystem.current.currentSelectedGameObject;
            if(currentGameObject!= null && currentGameObject.name != "Back Settings") {
                SnapTo(EventSystem.current.currentSelectedGameObject);
            }
        }
    }

    public void SnapTo(GameObject target) {
        int index = listItems.IndexOf(target); //Inventory Children List contains all the Children of the ScrollView's Content. We are getting the index of the selected one.
       
        if(index < 0) {
            UnityEngine.Debug.LogWarning("Could not find the element to SnapTo");
            return;
        }  
        
        RectTransform rect = listItems[index].GetComponent<RectTransform>(); //We are getting the RectTransform of the selected Inventory Item.
        bool inView = RectTransformUtility.RectangleContainsScreenPoint(scrollRect, rect.position); //We are checking if the Selected Inventory Item is visible from the camera.
        float buttonHeight = rect.rect.height;

        if (!inView) { //If the selected Item is not visible.
            if (oldRect != null) { //If we haven't assigned Old before we do nothing.
                if (oldRect.localPosition.y < rect.localPosition.y) { //If the last rect we were selecting is lower than our newly selected rect.
                    contentPanel.anchoredPosition += new Vector2(0, -buttonHeight); //We move the content panel down.
                }
                else if (oldRect.localPosition.y > rect.localPosition.y) { //if the last rect we were selecting is higher than our newly selected rect.
                    contentPanel.anchoredPosition += new Vector2(0, buttonHeight); //We move the content panel up.
                }
            }
        }
        oldRect = rect; //We assign our newly selected rect as the OldRect.
    }
}
