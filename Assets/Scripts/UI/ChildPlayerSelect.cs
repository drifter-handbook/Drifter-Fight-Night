using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildPlayerSelect : MonoBehaviour
{
    //Column then Row
    int totalRows = 2;
    int totalCols = 4;
    MainPlayerSelect m;
    // Start is called before the first frame update
    void Start()
    {
        m = GetComponent<MainPlayerSelect>();
    }

    // Update is called once per frame
    void Update()
    {
        int ID = GetComponent<NetworkID>().PlayerID;
        if (ID < 0 || ID >= m.CharacterSelectState.Count)
        {
            return;
        }
        if (!m.CharacterSelectState[ID].locked)
        {
            CharacterSelectState state = m.CharacterSelectState[ID];
            if (Input.GetKeyDown("right"))
            {
                state.x += 1;
            }
            else if (Input.GetKeyDown("left"))
            {
                state.x -= 1;
            }
            else if (Input.GetKeyDown("up"))
            {
                state.y -= 1;
            }
            else if (Input.GetKeyDown("down"))
            {
                state.y += 1;
            }
            state.x = (state.x + totalCols) % totalCols;
            state.y = (state.y + totalRows) % totalRows;
        }
        if (Input.GetKeyDown("l"))
        {
            m.CharacterSelectState[ID].locked = !m.CharacterSelectState[ID].locked;
        }
    }
}
