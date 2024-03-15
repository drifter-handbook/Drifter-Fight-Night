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
            if(currentGameObject!= null) {
                SnapTo();
            }
        }
    }

    public void SnapTo() {
        int index = listItems.IndexOf(EventSystem.current.currentSelectedGameObject); //Inventory Children List contains all the Children of the ScrollView's Content. We are getting the index of the selected one.
       
        if(index < 0) {
            Debug.LogWarning("Could not find the element to SnapTo");
            return;
        }  
        
        GameObject rect = EventSystem.current.currentSelectedGameObject; //We are getting the RectTransform of the selected Inventory Item.
        float viewing_top_y = scrollRect.position.y;
        float viewing_bottom_y = scrollRect.position.y + (scrollRect.rect.height);
        bool inView = rect.transform.position.y > viewing_top_y && rect.transform.position.y < viewing_bottom_y;
        float buttonHeight = rect.GetComponent<RectTransform>().rect.height;

        if (!inView) { //If the selected Item is not visible.
                if (rect.transform.position.y < viewing_top_y) { //If the last rect we were selecting is lower than our newly selected rect.
                    Debug.Log("Moving content down. rect.position.y = " + rect.transform.position.y);
                    contentPanel.anchoredPosition += new Vector2(0, buttonHeight); //We move the content panel down.
                }
                else if (rect.transform.position.y > viewing_bottom_y) { //if the last rect we were selecting is higher than our newly selected rect.
                    Debug.Log("Moving content up. rect.position.y = " + rect.transform.position.y);
                    contentPanel.anchoredPosition += new Vector2(0, -buttonHeight); //We move the content panel up.
                }
        }
    }
}
