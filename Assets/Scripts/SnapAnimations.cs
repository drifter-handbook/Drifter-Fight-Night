using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapAnimations : MonoBehaviour
{

    public TutorialSwapper info;
    public int myIndex = 0;
    GameObject childImage;


    // Start is called before the first frame update
    void Start()
    {
        childImage = this.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //hopefully this'll overwrite the animator, otherwise I'll clone the animator and strip positions
        //but I'd rather not, so let's get clever here

        childImage.transform.localPosition = new Vector3(0, 0, 0);


        //Some vertical chars need custom positions, even on buttons
        if(info.currentDrifterIndex == 3)
        {
            if(myIndex == 1 || myIndex == 4 )
            {
                childImage.transform.localPosition = new Vector3(0, 30, 0);
            }

            if (myIndex == 3 ||  myIndex == 5)
            {
                childImage.transform.localPosition = new Vector3(0, 40, 0);
            }
        }

    }
}
