using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ClearableCell : MonoBehaviour
{
    public bool IsBeingCleared { get; private set; }
    
    protected Cell cell;

    private void Awake()
    {
        cell = GetComponent<Cell>();
    }

    public virtual void Clear()
    {
        IsBeingCleared = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (SoundManager.instance.isPlayClearSound == false)
        {
            SoundManager.instance.PlaySFX("Clear");
            SoundManager.instance.isPlayClearSound = true;
        }
            
        GameObject temp = FXManager.instance.PlayEffect("Clear", this.transform.position, quaternion.identity);
        cell.ShapeComponent._sprite.sprite = null;

        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
        Destroy(temp);
        SoundManager.instance.isPlayClearSound = false;
    }
}
