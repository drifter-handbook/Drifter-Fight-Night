using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles input
public class PlayerInput : MonoBehaviour
{
    public PlayerInputData input;
    public CustomControls keyBindings;
    float timeOfFirstButton = 0f;
    bool firstButtonPressed = false;

    // Update is called once per frame
    void Update()
    {

        if(Time.time - timeOfFirstButton>1f) firstButtonPressed = false;

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
        if (Input.GetKey(keyBindings.downKey))
        {
            // down key does nothing
            input.MoveY--;
        }
        if (Input.GetKey(keyBindings.upKey))
        {
            input.MoveY++;
        }


        if(Input.GetKey(keyBindings.guard1Key) && Input.GetKey(keyBindings.downKey)){
            input.MoveY--;
        }


        input.Jump = Input.GetKey(keyBindings.jumpKey);
        input.Light = Input.GetKey(keyBindings.lightKey);
        input.Special = Input.GetKey(keyBindings.specialKey);
        input.Grab = Input.GetKey(keyBindings.grabKey);
        input.Guard = Input.GetKey(keyBindings.guard1Key) || Input.GetKey(keyBindings.guard2Key);
    }
}
