using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles input
public class PlayerInput : MonoBehaviour
{
    public PlayerInputData input;

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
        if (Input.GetKey(KeyCode.A))
        {
            input.MoveX--;
        }
        if (Input.GetKey(KeyCode.D))
        {
            input.MoveX++;
        }
        input.MoveY = 0;
        if (Input.GetKey(KeyCode.W))
        {
            input.MoveY = 1;
        }
    }
}
