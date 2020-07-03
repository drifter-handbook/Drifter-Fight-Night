using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildPlayerSelect : MonoBehaviour
{
    //Column then Row
    int totalRows = 2;
    int totalCols = 4;
    int rowNumb = 1;
    int rowCol = 1;
    bool locked = false;
    public MainPlayerSelect m;
    public int playerNumber = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!locked)
        {
            if (Input.GetKeyDown("right"))
            {
                goRight();
            }
            else if (Input.GetKeyDown("left"))
            {
                goLeft();
            }
            else if (Input.GetKeyDown("up"))
            {
                goUp();
            }
            else if (Input.GetKeyDown("down"))
            {
                goDown();
            }
        }

        if (Input.GetKeyDown("l"))
        {
            locked = !locked;
            m.locked[0] = !m.locked[0];
        }
    }

    void goRight()
    {
        rowCol++;
        if (rowCol == totalCols + 1)
        {
            rowCol = 1;
        }
        m.selectionArray[playerNumber-1, 1] = rowCol;
    }

    void goLeft()
    {
        rowCol--;
        if (rowCol == 0)
        {
            rowCol = totalCols;
        }
        m.selectionArray[playerNumber-1, 1] = rowCol;
    }

    void goUp()
    {
        rowNumb--;
        if (rowNumb == 0)
        {
            rowNumb = totalRows;
        }
        m.selectionArray[playerNumber-1, 0] = rowNumb;
    }

    void goDown()
    {
        rowNumb++;
        if (rowNumb == totalRows + 1)
        {
            rowNumb = 1;
        }
        m.selectionArray[playerNumber-1, 0] = rowNumb;
    }

}
