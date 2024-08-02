using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private int _x;
    private int _y;

    public int X
    {
        get => _x;
        set
        {
            if (IsMovable())
            {
                _x = value;
            }
        }
    }

    public int Y
    {
        get => _y;
        set
        {
            if (IsMovable())
            {
                _y = value;
            }
        }
    }

    public BoardController Board { get; private set; }

    public CellType Type { get; private set; }
    
    public MovableCell MovableComponent { get; private set; }
    public ClearableCell ClearableComponent { get; private set; }
    public ShapeCell ShapeComponent { get; private set; }

    private void Awake()
    {
        MovableComponent = GetComponent<MovableCell>();
        ClearableComponent = GetComponent<ClearableCell>();
        ShapeComponent = GetComponent<ShapeCell>();
    }

    public void Init(int x, int y, BoardController board,
                    CellType type = CellType.Empty)
    {
        _x = x;
        _y = y;
        Board = board;
        Type = type;
    }

    private void OnMouseEnter() => Board.EnterPiece(this);

    private void OnMouseDown() => Board.PressPiece(this);

    private void OnMouseUp() => Board.ReleasePiece();

    public bool IsMovable() => MovableComponent;
    
    public bool IsShaped() => ShapeComponent;
    
    public bool IsClearable() => ClearableComponent;
}
