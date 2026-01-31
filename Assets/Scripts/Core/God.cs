using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class God : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    
    [Header("Favor")]
    public float maxFavor = 100f;
    public float currentFavor = 50f;
    
    [Header("Over Time Effects")]
    public float bleedDPS = 0f;   // Damage per second (bleeding)
    public float regenHPS = 0f;   // Healing per second (regeneration)

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // Apply over-time effects
        if (bleedDPS > 0f)
        {
            ApplyDamage(bleedDPS * Time.deltaTime);
        }
        if (regenHPS > 0f)
        {
            ApplyHealing(regenHPS * Time.deltaTime);
        }
    }

    public void ApplyDamage(float value)
    {
        currentHealth = Mathf.Max(0f, currentHealth - value);
    }

    public void ApplyHealing(float value)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + value);
    }

    public void ModifyFavor(float value)
    {
        currentFavor = Mathf.Clamp(currentFavor + value, 0f, maxFavor);
    }

    public void SetBleed(float dps)
    {
        bleedDPS = Mathf.Max(0f, dps);
    }

    public void SetRegen(float hps)
    {
        regenHPS = Mathf.Max(0f, hps);
    }
}
