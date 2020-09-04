using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class TutorialSwapper : MonoBehaviour
{
    public GameObject[] buttons = new GameObject[8];
    public GameObject[] controls = new GameObject[8];
    public DrifterTutorialInfo[] tutorialInfo = new DrifterTutorialInfo[8];
   

    [Serializable]
    public class DrifterTutorialInfo
    {
        public string drifterName;
        public bool flipSprites;
        public string[] moveTitles = new string[8];
        public string[] moveDescriptions = new string[8]; 
    }

    //Attack (Q)
    //Neutral Aerial (Q + Air)
    //Grab (E)
    //Neutral Special (W)
    //Side Special (W + L/R)
    //Down Special (W + Down)
    //Recovery (W + Up)
    //Guard (Shift)

    public Animator mainPreviewAnimator;
    public Text moveTitle;
    public Text moveDescription;

    public Text name;
    public Text nameShadow;

    private int currentDrifterIndex = 0;
    private DrifterTutorialInfo currentDrifter;


    // Start is called before the first frame update
    void Start()
    {
        setupDrifter();
    }

    private void setupDrifter()
    {
        currentDrifter = tutorialInfo[currentDrifterIndex];
        name.text = currentDrifter.drifterName;
        nameShadow.text = currentDrifter.drifterName;
        int startMove = 1;
        mainPreviewAnimator.SetInteger("move", 0);
        mainPreviewAnimator.SetInteger("char", currentDrifterIndex);
        foreach (GameObject btn in buttons)
        {
            btn.GetComponent<Animator>().SetInteger("move", startMove);
            startMove++;
            btn.GetComponent<Animator>().SetInteger("char", currentDrifterIndex);
            
        }

        if (currentDrifter.flipSprites)
        {
            mainPreviewAnimator.gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            mainPreviewAnimator.gameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    
    public void nextDrifter()
    {
        currentDrifterIndex++;
       
        if (currentDrifterIndex >= tutorialInfo.Length)
        {
            currentDrifterIndex = 0;
        }
        currentDrifter = tutorialInfo[currentDrifterIndex];
        setupDrifter();
    }

    public void prevDrifter()
    {
        currentDrifterIndex--;
       
        if (currentDrifterIndex < 0)
        {
            currentDrifterIndex = tutorialInfo.Length-1;
        }
        currentDrifter = tutorialInfo[currentDrifterIndex];
        setupDrifter();
    }

    public void ChooseSkill(int index)
    {
        foreach(GameObject control in controls)
        {
            control.SetActive(false);
        }
        moveTitle.text = currentDrifter.moveTitles[index];

        moveDescription.text = currentDrifter.moveDescriptions[index];
        controls[index].SetActive(true);
        mainPreviewAnimator.SetInteger("move",index+1);
    }
}
