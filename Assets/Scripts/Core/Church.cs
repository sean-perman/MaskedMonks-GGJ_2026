using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Church : MonoBehaviour
{
    [Header("Room Properties")]
    public float maxCondition = 100f;
    public float currentCondition = 100f;

    // Start is called before the first frame update
    void Start()
    {
        currentCondition = maxCondition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyDamage(float value)
    {
        currentCondition = Mathf.Max(0f, currentCondition - value);
    }

    public void ApplyRepair(float value)
    {
        currentCondition = Mathf.Min(maxCondition, currentCondition + value);
    }
}
