using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncCameraClient : MonoBehaviour, ISyncClient
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
        SyncableCameraZoom cameraZoom = NetworkUtils.GetNetworkData<SyncableCameraZoom>(sync["camera_zoom"]);
        if (cameraZoom != null)
        {
             cam.orthographicSize = cameraZoom.zoom;
        }
    }
}
