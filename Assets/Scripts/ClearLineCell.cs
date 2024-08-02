using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLineCell : ClearableCell
{
    public bool isRow;

    public override void Clear()
    {
        base.Clear();

        if (isRow)
        {            
            cell.Board.ClearRow(cell.Y);
        }
        else
        {            
            cell.Board.ClearColumn(cell.X);
        }
    }
}
