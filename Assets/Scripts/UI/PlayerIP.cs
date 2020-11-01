using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class PlayerIP : MonoBehaviour
{

    public Text hostIP;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hostIP != null)
        {
            if (GameController.Instance.IsHost)
            {
                string holepunch_ip = Resources.Load<TextAsset>("Config/server_ip").text.Trim();
                hostIP.text = $"{GameController.Instance.GetComponent<IPWebRequest>().result.ToString()}:{UDPHolePuncher.GetLocalIP(holepunch_ip, 6970).GetAddressBytes()[3]}";
            }
            else
            {
                hostIP.text = $"{GameController.Instance.GetComponent<NetworkClient>().Network.hostIP.ToString()}:{GameController.Instance.GetComponent<NetworkClient>().Network.hostID}";
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            string holepunch_ip = Resources.Load<TextAsset>("Config/server_ip").text.Trim();
            GUIUtility.systemCopyBuffer = $"{GameController.Instance.GetComponent<IPWebRequest>().result.ToString()}:{UDPHolePuncher.GetLocalIP(holepunch_ip, 6970).GetAddressBytes()[3]}";
        }

    }
}
