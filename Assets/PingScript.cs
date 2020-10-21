using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("HidePing") > 0)
        {
           gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
