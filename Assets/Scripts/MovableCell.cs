using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableCell : MonoBehaviour
{
    private Cell _piece;
    private IEnumerator _moveCoroutine;

    private void Awake()
    {
        _piece = GetComponent<Cell>();
    }

    public void Move(int newX, int newY, float time, bool isReturn = false)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        _moveCoroutine = MoveCoroutine(newX, newY, time, isReturn);
        StartCoroutine(_moveCoroutine);
    }

    private IEnumerator MoveCoroutine(int newX, int newY, float time, bool isReturn)
    {

        _piece.X = newX;
        _piece.Y = newY;

        Vector3 startPos = transform.position;
        Vector3 endPos = _piece.Board.GetWorldPosition(newX, newY);

        for (float t = 0; t <= 1 * time; t += Time.deltaTime)
        {
            _piece.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return null;
        }
        
        _piece.transform.position = endPos;

        if (isReturn)
        {
            for (float t = 0; t <= 1 * time; t += Time.deltaTime)
            {
                _piece.transform.position = Vector3.Lerp(endPos, startPos, t / time);
                yield return null;
            }
            
            _piece.transform.position = startPos;
        }
    }
}

