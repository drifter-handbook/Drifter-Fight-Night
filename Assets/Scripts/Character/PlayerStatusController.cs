using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    // Start is called before the first frame update

    public PlayerStatus status;
    public int mode;
    public Animator anim;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("Status",mode);
        mode = status.GetStatusToRender();                
    }

}
