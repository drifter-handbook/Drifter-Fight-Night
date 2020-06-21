using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPService : MonoBehaviour
{
    public UDPNetwork Network { get; private set; }
    public string target;

    // Start is called before the first frame update
    void Start()
    {
        Network = new UDPNetwork(target, "minecraft.scrollingnumbers.com", 6969);
        StartCoroutine(Network.Connect());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnApplicationQuit()
    {
        Network?.Kill();
    }
}
