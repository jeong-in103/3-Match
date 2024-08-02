using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearShapeCell : ClearableCell
{
    public CellShapeType Shape { get; set; }

    public override void Clear()
    {
        base.Clear();

        cell.Board.ClearShape(Shape);
    }
}
