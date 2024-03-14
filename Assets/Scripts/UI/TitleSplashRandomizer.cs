using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleSplashRandomizer : MonoBehaviour {

	public Sprite[] splashes;
	public Image currentSplash;
    // Start is called before the first frame update
    void Start() {
        currentSplash.sprite = splashes[(int)Random.Range(0,splashes.Length-1)];
    }   
}
