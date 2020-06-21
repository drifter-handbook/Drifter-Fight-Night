using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkHost : MonoBehaviour
{
    UDPService service;

    // advertise as host to hole punch server
    private void Awake()
    {
        service = GetComponent<UDPService>();
        service.target = "minecraft.scrollingnumbers.com";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
