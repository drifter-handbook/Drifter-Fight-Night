using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles input
public class PlayerInput : MonoBehaviour
{
    public PlayerInputData input;
    public CustomControls keyBindings;

    // Update is called once per frame
    void Update()
    {

        if (input == null)
        {
            return;
        }
        // get player input

        //controller movement input
        if(Input.GetJoystickNames().Length > 0)
        {
            input.MoveX = Input.GetAxis("Horizontal");
            input.MoveY = Input.GetAxis("Vertical");
        }

        //keyboard movement input
        else
        {
            input.MoveX = 0;
            if (Input.GetKey(keyBindings.leftKey))
            {
                input.MoveX--;
            }
            if (Input.GetKey(keyBindings.rightKey) || Input.GetButtonDown("Horizontal"))
            {
                input.MoveX++;
            }
            input.MoveY = 0;
            if (Input.GetKey(keyBindings.downKey))
            {
                // down key does nothing
                input.MoveY--;
            }
            if (Input.GetKey(keyBindings.upKey))
            {
                input.MoveY++;
            }
        }

        if (Input.GetKey(keyBindings.guard1Key) && Input.GetKey(keyBindings.downKey)){
            input.MoveY--;
        }


        input.Jump = Input.GetKey(keyBindings.jumpKey);
        input.Light = Input.GetKey(keyBindings.lightKey);
        input.Special = Input.GetKey(keyBindings.specialKey);
        input.Grab = Input.GetKey(keyBindings.grabKey);
        input.Guard = Input.GetKey(keyBindings.guard1Key) || Input.GetKey(keyBindings.guard2Key);
    }
}
