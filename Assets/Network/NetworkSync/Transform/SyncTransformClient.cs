using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTransformClient : MonoBehaviour, ISyncClient
{
    NetworkSync sync;

    // Start is called before the first frame update
    void Start()
    {
        if(!GameController.Instance.IsOnline)return;
        sync = GetComponent<NetworkSync>();
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameController.Instance.IsOnline)return;
        SyncableTransform2D netTransform = NetworkUtils.GetNetworkData<SyncableTransform2D>(sync["transform"]);
        if (netTransform != null)
        {
            transform.position = netTransform.position.ToVector3();
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, netTransform.rotation);
            Vector2 netScale = netTransform.scale.ToVector2();
            transform.localScale = new Vector3(netScale.x, netScale.y, transform.localScale.z);
        }
    }
}
