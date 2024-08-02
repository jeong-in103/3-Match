using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeCell : MonoBehaviour
{   
    [System.Serializable]
    public struct ShapeSprite
    {
        public CellShapeType shape;
        public Sprite sprite;
    }

    public ShapeSprite[] shapeSprites;

    private CellShapeType _shape;

    public CellShapeType Shape
    {
        get => _shape;
        set => SetShape(value);
    }

    public int NumShape => shapeSprites.Length;

    public SpriteRenderer _sprite;
    private Dictionary<CellShapeType, Sprite> _shapeSpriteDict;

    private void Awake ()
    {
        _sprite = transform.Find("shape").GetComponent<SpriteRenderer>();

        // instantiating and populating a Dictionary of all Color Types / Sprites (for fast lookup)
        _shapeSpriteDict = new Dictionary<CellShapeType, Sprite>();

        for (int i = 0; i < shapeSprites.Length; i++)
        {
            if (!_shapeSpriteDict.ContainsKey (shapeSprites[i].shape))
            {
                _shapeSpriteDict.Add(shapeSprites[i].shape, shapeSprites[i].sprite);
            }
        }
    }

    public void SetShape(CellShapeType newShape)
    {
        _shape = newShape;

        if (_shapeSpriteDict.ContainsKey(newShape))
        {
            _sprite.sprite = _shapeSpriteDict[newShape];
        }
    }
}
