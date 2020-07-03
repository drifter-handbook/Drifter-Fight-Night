using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class IPWebRequest : MonoBehaviour
{
    public long status;
    // result ready but not yet fetched
    public bool complete = true;

    public IPAddress result;

    void Start()
    {
        complete = true;
        RequestIP();
    }

    public void RequestIP()
    {
        complete = false;
        StartCoroutine(SendIPRequest());
    }

    IEnumerator SendIPRequest()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.ipify.org"))
        {
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                complete = true;
                status = www.responseCode;
                result = IPAddress.Parse(www.downloadHandler.text);
                Debug.Log($"Our own IP is {result}");
            }
            else
            {
                complete = true;
                status = www.responseCode;
            }
        }
    }
}
