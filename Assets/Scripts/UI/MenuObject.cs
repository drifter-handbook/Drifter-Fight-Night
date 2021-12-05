using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuObject : MonoBehaviour
{

	//Character Matrix
    GameObject[][] characterRows = new GameObject[3][];
    MatchmakingUI matchMaker;

    public GameObject[] Local;
    public GameObject[] Online;
    public GameObject[] Settings;
    public GameObject[] Exit;

    //Circular Array Helper
    private int wrapIndex(int curr, int max)
    {
        if(curr >= max) return 0;
        else if(curr < 0) return (max-1);
        else return curr;
    }


    // Start is called before the first frame update
    void Awake()
    {
    	matchMaker = GetComponent<MatchmakingUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
