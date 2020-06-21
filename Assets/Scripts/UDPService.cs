using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPService : MonoBehaviour
{
    UDPNetwork network;

    // Start is called before the first frame update
    void Start()
    {
        network = new UDPNetwork("68.187.67.135", "minecraft.scrollingnumbers.com", 6969);
        StartCoroutine(network.Connect());
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log($"State: ing: {client.Connecting}, ed: {client.Connected}, f: {client.Failed}");
    }

    void OnApplicationQuit()
    {
        network?.Kill();
    }
}
