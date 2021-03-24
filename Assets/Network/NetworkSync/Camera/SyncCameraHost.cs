using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncCameraHost : MonoBehaviour, ISyncHost
{
    NetworkSync sync;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
        cam = GetComponent<Camera>();
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        sync["camera_zoom"] = new SyncableCameraZoom()
        {
            zoom = cam.orthographicSize
        };
    }
}

public class SyncableCameraZoom : INetworkData
{
    public float zoom;
    public string Type { get; set; }
}