using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

// 실제 플레이 보드 관련
// 1. 보드 생성 및 초기화
// 2. cell 이동 및 3 match 판정
// 3. 특수 아이템 생성
public class BoardController : MonoBehaviour
{
    [System.Serializable]
    public struct CellPrefab
    {
        public CellType type;
        public GameObject prefab;
    };
    
    [SerializeField] private int _xSize;
    [SerializeField] private int _ySize;
    [SerializeField] private float _fillTime;

    [SerializeField] private CellPrefab[] _cellPrefabs;
    private Dictionary<CellType, GameObject> _cellPrefabDict;
    
    private Cell[,] _cells;

    private bool _inverse;
    
    private Cell _pressedCell;
    private Cell _enteredCell;

    public bool IsFilling { get; private set; }

    private void Awake()
    {
        // populating dictionary with piece prefabs types
        _cellPrefabDict = new Dictionary<CellType, GameObject>();
        for (int i = 0; i < _cellPrefabs.Length; i++)
        {
            if (!_cellPrefabDict.ContainsKey(_cellPrefabs[i].type))
            {
                _cellPrefabDict.Add(_cellPrefabs[i].type, _cellPrefabs[i].prefab);
            }
        }
        
        _cells = new Cell[_xSize, _ySize];
        
        for (int x = 0; x < _xSize; x++)
        {
            for (int y = 0; y < _ySize; y++)
            {
                if (_cells[x, y] == null)
                {
                    SpawnNewCell(x, y, CellType.Empty);
                }
            }
        }
        
        StartCoroutine(Fill());
    }
    
    private static bool IsAdjacent(Cell piece1, Cell piece2) =>
            (piece1.X == piece2.X && Mathf.Abs(piece1.Y - piece2.Y) == 1) ||
            (piece1.Y == piece2.Y && Mathf.Abs(piece1.X - piece2.X) == 1);
    
    private void SwapPieces(Cell piece1, Cell piece2)
    {
        if (!piece1.IsMovable() || !piece2.IsMovable()) return;
        
        _cells[piece1.X, piece1.Y] = piece2;
        _cells[piece2.X, piece2.Y] = piece1;

        List<Cell> result1 = GetMatch(piece1, piece2.X, piece2.Y);
        List<Cell> result2 = GetMatch(piece2, piece1.X, piece1.Y);

        bool isReturn = !(result1 != null || result2 != null);
        if (piece1.Type == CellType.Rainbow || piece2.Type == CellType.Rainbow ||
            piece1.Type == CellType.ColumnClear || piece2.Type == CellType.ColumnClear ||
            piece1.Type == CellType.RowClear || piece2.Type == CellType.RowClear)
        {
            isReturn = false;
        }
        
        int piece1X = piece1.X;
        int piece1Y = piece1.Y;
            
        piece1.MovableComponent.Move(piece2.X, piece2.Y, _fillTime, isReturn); 
        piece2.MovableComponent.Move(piece1X, piece1Y, _fillTime, isReturn);
        SoundManager.instance.PlaySFX("Swap");
            
        if (piece1.Type == CellType.Rainbow && piece1.IsClearable() && piece2.IsShaped()) 
        { 
            ClearShapeCell clearShape = piece1.GetComponent<ClearShapeCell>();
            
            if (clearShape) 
            { 
                clearShape.Shape = piece2.ShapeComponent.Shape;
            }
            
            ClearCell(piece1.X, piece1.Y);
        }
        
        if (piece2.Type == CellType.Rainbow && piece2.IsClearable() && piece1.IsShaped()) 
        { 
            ClearShapeCell clearShape = piece2.GetComponent<ClearShapeCell>();
            
            if (clearShape) 
            { 
                clearShape.Shape = piece1.ShapeComponent.Shape;
            }
            
            ClearCell(piece2.X, piece2.Y);
        }
        
        ClearAllValidMatches();
        
        // special pieces get cleared, event if they are not matched
        if (piece1.Type == CellType.RowClear || piece1.Type == CellType.ColumnClear) 
        { 
            ClearCell(piece1.X, piece1.Y);
        } 
        
        if (piece2.Type == CellType.RowClear || piece2.Type == CellType.ColumnClear) 
        { 
            ClearCell(piece2.X, piece2.Y);
        }
            
        _pressedCell = null; 
        _enteredCell = null;
            
        StartCoroutine(Fill());
    }
    
    public void PressPiece(Cell piece) => _pressedCell = piece;

    public void EnterPiece(Cell piece) => _enteredCell = piece;

    public void ReleasePiece()
    {
        if (IsAdjacent (_pressedCell, _enteredCell))
        {
            SwapPieces(_pressedCell, _enteredCell);
        }
    }

    private IEnumerator Fill()
    {
        bool isNeedsRefill = true;
        IsFilling = true;
        
        while (isNeedsRefill)
        {
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(_fillTime);
            while (FillStep())
            {
                _inverse = !_inverse;
                yield return new WaitForSeconds(_fillTime);
            }

            isNeedsRefill = ClearAllValidMatches();
        }
        
        IsFilling = false;
    }

    private bool FillStep()
    {
        bool movedPiece = false;
        // y = 0 is at the top, we ignore the last row, since it can't be moved down.
        for (int y = _ySize - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < _xSize; loopX++)
            {
                int x = loopX;
                if (_inverse) { x = _xSize - 1 - loopX; }
                Cell cell = _cells[x, y];

                if (!cell.IsMovable()) continue;
                
                Cell below = _cells[x, y + 1];
                
                if (below.Type == CellType.Empty)
                {
                    Destroy(below.gameObject);
                    cell.MovableComponent.Move(x, y + 1, _fillTime);
                    _cells[x, y + 1] = cell;
                    SpawnNewCell(x, y, CellType.Empty);
                    movedPiece = true;
                }
                else
                {
                    for (int diag = -1; diag <= 1; diag++)
                    {
                        if (diag == 0) continue;
                        
                        int diagX = x + diag;

                        if (_inverse)
                        { 
                            diagX = x - diag;
                        }

                        if (diagX < 0 || diagX >= _xSize) continue;
                        
                        Cell diagonalCell = _cells[diagX, y + 1];

                        if (diagonalCell.Type != CellType.Empty) continue;
                        
                        bool hasPieceAbove = true;

                        for (int aboveY = y; aboveY >= 0; aboveY--)
                        {
                            Cell pieceAbove = _cells[diagX, aboveY];

                            if (pieceAbove.IsMovable())
                            {
                                break;
                            }
                            else if (/*!pieceAbove.IsMovable() && */pieceAbove.Type != CellType.Empty)
                            {
                                hasPieceAbove = false;
                                break;
                            }
                        }

                        if (hasPieceAbove) continue;
                        
                        Destroy(diagonalCell.gameObject);
                        cell.MovableComponent.Move(diagX, y + 1, _fillTime);
                        _cells[diagX, y + 1] = cell;
                        SpawnNewCell(x, y, CellType.Empty);
                        movedPiece = true;
                        break; 
                    }
                }
            }
        }

        // the highest row (0) is a special case, we must fill it with new pieces if empty
        for (int x = 0; x < _xSize; x++)
        {
            Cell cellBelow = _cells[x, 0];

            if (cellBelow.Type != CellType.Empty) continue;
            
            Destroy(cellBelow.gameObject);
            GameObject newPiece = Instantiate(_cellPrefabDict[CellType.Basic], GetWorldPosition(x, -1), Quaternion.identity, this.transform);

            _cells[x, 0] = newPiece.GetComponent<Cell>();
            _cells[x, 0].Init(x, -1, this, CellType.Basic);
            _cells[x, 0].MovableComponent.Move(x, 0, _fillTime);
            _cells[x, 0].ShapeComponent.SetShape((CellShapeType)Random.Range(0, _cells[x, 0].ShapeComponent.NumShape));
            movedPiece = true;
        }

        return movedPiece;
    }
    
    private List<Cell> GetMatch(Cell piece, int newX, int newY)
    {
        if (!piece.IsShaped()) return null;
        var color = piece.ShapeComponent.Shape;
        var horizontalPieces = new List<Cell>();
        var verticalPieces = new List<Cell>();
        var matchingPieces = new List<Cell>();

        // First check horizontal
        horizontalPieces.Add(piece);

        for (int dir = 0; dir <= 1; dir++)
        {
            for (int xOffset = 1; xOffset < _xSize; xOffset++)
            {
                int x;

                if (dir == 0)
                { // Left
                    x = newX - xOffset;
                }
                else
                { // right
                    x = newX + xOffset;                        
                }

                // out-of-bounds
                if (x < 0 || x >= _xSize) { break; }

                // piece is the same color?
                if (_cells[x, newY].IsShaped() && _cells[x, newY].ShapeComponent.Shape == color)
                {
                    horizontalPieces.Add(_cells[x, newY]);
                }
                else
                {
                    break;
                }
            }
        }

        if (horizontalPieces.Count >= 3)
        {
            matchingPieces.AddRange(horizontalPieces);
        }

        // Traverse vertically if we found a match (for L and T shape)
        if (horizontalPieces.Count >= 3)
        {
            foreach (var t in horizontalPieces)
            {
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int yOffset = 1; yOffset < _ySize; yOffset++)                        
                    {
                        int y;
                        
                        if (dir == 0)
                        { // Up
                            y = newY - yOffset;
                        }
                        else
                        { // Down
                            y = newY + yOffset;
                        }

                        if (y < 0 || y >= _ySize)
                        {
                            break;
                        }

                        if (_cells[t.X, y].IsShaped() && _cells[t.X, y].ShapeComponent.Shape == color)
                        {
                            verticalPieces.Add(_cells[t.X, y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (verticalPieces.Count < 2)
                {
                    verticalPieces.Clear();
                }
                else
                {
                    matchingPieces.AddRange(verticalPieces);
                    break;
                }
            }
        }

        if (matchingPieces.Count >= 3)
        {
            return matchingPieces;
        }


        // Didn't find anything going horizontally first,
        // so now check vertically
        horizontalPieces.Clear();
        verticalPieces.Clear();
        verticalPieces.Add(piece);

        for (int dir = 0; dir <= 1; dir++)
        {
            for (int yOffset = 1; yOffset < _xSize; yOffset++)
            {
                int y;

                if (dir == 0)
                { // Up
                    y = newY - yOffset;
                }
                else
                { // Down
                    y = newY + yOffset;                        
                }

                // out-of-bounds
                if (y < 0 || y >= _ySize) { break; }

                // piece is the same color?
                if (_cells[newX, y].IsShaped() && _cells[newX, y].ShapeComponent.Shape == color)
                {
                    verticalPieces.Add(_cells[newX, y]);
                }
                else
                {
                    break;
                }
            }
        }

        if (verticalPieces.Count >= 3)
        {
            matchingPieces.AddRange(verticalPieces);
        }

        // Traverse horizontally if we found a match (for L and T shape)
        if (verticalPieces.Count >= 3)
        {
            foreach (var t in verticalPieces)
            {
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int xOffset = 1; xOffset < _ySize; xOffset++)
                    {
                        int x;

                        if (dir == 0)
                        { // Left
                            x = newX - xOffset;
                        }
                        else
                        { // Right
                            x = newX + xOffset;
                        }

                        if (x < 0 || x >= _xSize)
                        {
                            break;
                        }

                        if (_cells[x, t.Y].IsShaped() && _cells[x, t.Y].ShapeComponent.Shape == color)
                        {
                            horizontalPieces.Add(_cells[x, t.Y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (horizontalPieces.Count < 2)
                {
                    horizontalPieces.Clear();
                }
                else
                {
                    matchingPieces.AddRange(horizontalPieces);
                    break;
                }
            }
        }

        if (matchingPieces.Count >= 3)
        {
            return matchingPieces;
        }

        return null;
    }
    
     private bool ClearAllValidMatches()
     {
         bool needsRefill = false;

         for (int y = 0; y < _ySize; y++)
         {
             for (int x = 0; x < _xSize; x++)
             {
                 if (!_cells[x, y].IsClearable()) continue;
                
                 List<Cell> match = GetMatch(_cells[x, y], x, y);

                 if (match == null) continue;
                
                 CellType specialCellType = CellType.Count;
                 Cell randomCell = match[Random.Range(0, match.Count)];
                 int specialCellX = randomCell.X;
                 int specialCellY = randomCell.Y;

                 // Spawning special pieces
                 if (match.Count == 4)
                 {
                      if (_pressedCell == null || _enteredCell == null)
                      { 
                          specialCellType = (CellType) Random.Range((int)CellType.RowClear, (int)CellType.ColumnClear);
                      }
                      else if (_pressedCell.Y == _enteredCell.Y)
                      { 
                          specialCellType = CellType.RowClear;
                      }
                      else
                      {
                          specialCellType = CellType.ColumnClear;
                      }
                 } // Spawning a rainbow piece
                 else if (match.Count >= 5)
                 {
                     specialCellType = CellType.Rainbow;
                 }

                 foreach (var cell in match)
                 {
                     if (!ClearCell(cell.X, cell.Y)) continue;
                    
                     needsRefill = true;

                     if (cell != _pressedCell && cell != _enteredCell) continue;
                    
                     specialCellX = cell.X;
                     specialCellY = cell.Y;
                 }

                 // Setting their colors
                 if (specialCellType == CellType.Count) continue;
                
                 Destroy(_cells[specialCellX, specialCellY]);
                 Cell newCell = SpawnNewCell(specialCellX, specialCellY, specialCellType);
                 SoundManager.instance.PlaySFX("Item");

                 if ((specialCellType == CellType.RowClear || specialCellType == CellType.ColumnClear) 
                     && newCell.IsShaped() && match[0].IsShaped())
                 {
                     newCell.ShapeComponent.SetShape(CellShapeType.Line);

                     newCell.GetComponent<ClearLineCell>().isRow = specialCellType == CellType.RowClear;
                 }
                 else if (specialCellType == CellType.Rainbow && newCell.IsShaped()) 
                 { 
                     newCell.ShapeComponent.SetShape(CellShapeType.Any);
                 }
             }
         }

         return needsRefill;
     }
     
     private bool ClearCell(int x, int y)
     {
         if (!_cells[x, y].IsClearable() || _cells[x, y].ClearableComponent.IsBeingCleared) return false;
  
         _cells[x, y].ClearableComponent.Clear();
         SpawnNewCell(x, y, CellType.Empty);
         
         return true;

     }
     
     public void ClearShape(CellShapeType shape)
     {
         for (int x = 0; x < _xSize; x++)
         {
             for (int y = 0; y < _ySize; y++)
             {
                 if ((_cells[x, y].IsShaped() && _cells[x, y].ShapeComponent.Shape == shape)
                     || (shape == CellShapeType.Any))
                 {
                     ClearCell(x, y);
                 }
             }
         }
     }
     
     public void ClearRow(int row)
     {
         for (int x = 0; x < _xSize; x++)
         {
             ClearCell(x, row);
         }
     }

     public void ClearColumn(int column)
     {
         for (int y = 0; y < _ySize; y++)
         {
             ClearCell(column, y);
         }
     }

    private Cell SpawnNewCell(int x, int y, CellType type)
    {
        GameObject newCell = Instantiate(_cellPrefabDict[type], GetWorldPosition(x, y), quaternion.identity, this.transform);
        _cells[x, y] = newCell.GetComponent<Cell>();
        _cells[x, y].Init(x, y, this, type);

        return _cells[x, y];
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(
            transform.position.x - _xSize / 2.0f + 0.5f + x,
            transform.position.y + _ySize / 2.0f + 0.5f - y);
    }
}
