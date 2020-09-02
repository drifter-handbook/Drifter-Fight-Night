using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class TutorialSwapper : MonoBehaviour
{
    public GameObject[] buttons = new GameObject[8];
    public DrifterTutorialInfo[] tutorialInfo = new DrifterTutorialInfo[8];
   

    [Serializable]
    public class DrifterTutorialInfo
    {
        public string drifterName;
        public Animator animator; 
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



    private int currentDrifterIndex = 0;
    private DrifterTutorialInfo currentDrifter;


    // Start is called before the first frame update
    void Start()
    {
        currentDrifter = tutorialInfo[0];
        int startMove = 1;
        foreach(GameObject btn in buttons)
        {
            btn.GetComponent<Animator>().SetInteger("move", startMove);
            startMove++;
        }
    }

    
    public void ChooseSkill(int index)
    {
        moveTitle.text = currentDrifter.moveTitles[index];
        moveDescription.text = currentDrifter.moveDescriptions[index];
        mainPreviewAnimator.SetInteger("move",index+1);
        //TODO: swap controls based on index
    }
}
