using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager instance;

    public GameObject[] effectPrefabs;

    private Dictionary<string, GameObject> effectDict;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        effectDict = new Dictionary<string, GameObject>();
        foreach (var effect in effectPrefabs)
        {
            effectDict[effect.name] = effect;
        }
    }

    public GameObject PlayEffect(string effectName, Vector3 position, Quaternion rotation)
    {
        GameObject fx = null;
        if (effectDict.ContainsKey(effectName))
        {
            fx = Instantiate(effectDict[effectName], position, rotation, this.transform);
        }
        else
        {
            Debug.LogWarning("Effect not found: " + effectName);
        }

        return fx;
    }
}
