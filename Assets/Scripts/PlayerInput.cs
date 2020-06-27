using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles input
public class PlayerInput : MonoBehaviour
{
    public PlayerInputData input;
    public CustomControls keyBindings;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (input == null)
        {
            return;
        }
        // get player input
        input.MoveX = 0;
        if (Input.GetKey(keyBindings.leftKey))
        {
            input.MoveX--;
        }
        if (Input.GetKey(keyBindings.rightKey))
        {
            input.MoveX++;
        }
        input.MoveY = 0;
        if (Input.GetKey(keyBindings.upKey))
        {
            input.MoveY = 1;
        }
        input.Jump = input.Jump || Input.GetKeyDown(keyBindings.jumpKey);
        input.Light = input.Light || Input.GetKeyDown(keyBindings.lightKey);
        input.Grab = input.Grab || Input.GetKeyDown(keyBindings.grabKey);
        input.Guard = Input.GetKey(keyBindings.guard1Key) || Input.GetKey(keyBindings.guard2Key);
    }

    public void ResetKeyDowns()
    {
        input.Jump = false;
        input.Light = false;
        input.Grab = false;
    }
}
