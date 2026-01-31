using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The floating deity above each church.
/// Handles combat, mask management, and favor/strength resources.
/// </summary>
public class God : MonoBehaviour
{
    [Header("Strength (Health & Attack Power)")]
    [SerializeField] private int strength = 100;
    [SerializeField] private int maxStrength = 100;
    
    [Header("Favor")]
    [SerializeField] private int favor = 50;
    [SerializeField] private int maxFavor = 100;
    
    [Header("Masks")]
    [SerializeField] private Mask currentMask;
    [SerializeField] private List<Mask> storedMasks = new();
    [SerializeField] private int maxStoredMasks = 4;
    
    [Header("Over Time Effects")]
    [SerializeField] private float bleedDPS = 0f;
    [SerializeField] private float regenHPS = 0f;
    
    // Accumulator for fractional damage/healing
    private float damageAccumulator = 0f;
    private float healingAccumulator = 0f;
    
    // === Properties ===
    
    public int Strength => strength;
    public int MaxStrength => maxStrength;
    public int Favor => favor;
    public int MaxFavor => maxFavor;
    public Mask CurrentMask => currentMask;
    public IReadOnlyList<Mask> StoredMasks => storedMasks;
    public IReadOnlyList<Mask> Masks => storedMasks; // Alias for convenience
    public int MaskStorageRemaining => maxStoredMasks - storedMasks.Count;
    
    // === Initialization ===
    
    /// <summary>
    /// Initialize the god with starting stats.
    /// </summary>
    public void Initialize(int startStrength, int startFavor)
    {
        maxStrength = startStrength;
        strength = startStrength;
        maxFavor = startFavor > 0 ? startFavor * 2 : 100;
        favor = startFavor;
    }
    
    // === Unity Lifecycle ===
    
    private void Start()
    {
        strength = maxStrength;
    }

    private void Update()
    {
        // Apply over-time effects
        if (bleedDPS > 0f)
        {
            damageAccumulator += bleedDPS * Time.deltaTime;
            if (damageAccumulator >= 1f)
            {
                int damage = Mathf.FloorToInt(damageAccumulator);
                DecreaseStrength(damage);
                damageAccumulator -= damage;
            }
        }
        if (regenHPS > 0f)
        {
            healingAccumulator += regenHPS * Time.deltaTime;
            if (healingAccumulator >= 1f)
            {
                int healing = Mathf.FloorToInt(healingAccumulator);
                IncreaseStrength(healing);
                healingAccumulator -= healing;
            }
        }
        
        // Tick shelf life on stored masks
        for (int i = storedMasks.Count - 1; i >= 0; i--)
        {
            if (storedMasks[i] != null)
            {
                storedMasks[i].TickShelfLife(Time.deltaTime);
                if (storedMasks[i].IsExpired)
                {
                    Debug.Log($"Mask {storedMasks[i].Type} expired in storage!");
                    storedMasks.RemoveAt(i);
                }
            }
        }
    }
    
    // === Strength (Health/Attack) ===
    
    public void IncreaseStrength(int amount)
    {
        strength = Mathf.Min(maxStrength, strength + amount);
    }
    
    public void DecreaseStrength(int amount)
    {
        strength = Mathf.Max(0, strength - amount);
        
        if (strength <= 0)
        {
            OnStrengthDepleted();
        }
    }
    
    // === Favor ===
    
    public void IncreaseFavor(int amount)
    {
        favor = Mathf.Min(maxFavor, favor + amount);
    }
    
    public void DecreaseFavor(int amount)
    {
        favor = Mathf.Max(0, favor - amount);
        
        if (favor <= 0)
        {
            OnFavorDepleted();
        }
    }
    
    public bool CanAffordFavor(int cost)
    {
        return favor >= cost;
    }
    
    // === Mask Management ===
    
    /// <summary>
    /// Add a mask to storage if there's room.
    /// </summary>
    public bool AddMaskToStorage(Mask mask)
    {
        if (mask == null) return false;
        
        if (storedMasks.Count >= maxStoredMasks)
        {
            Debug.LogWarning("Mask storage is full!");
            return false;
        }
        
        storedMasks.Add(mask);
        return true;
    }
    
    /// <summary>
    /// Equip a mask from storage (replaces current mask).
    /// </summary>
    public bool SetMask(int storageIndex)
    {
        if (storageIndex < 0 || storageIndex >= storedMasks.Count)
        {
            return false;
        }
        
        // Unequip current mask (it's consumed/lost)
        currentMask = storedMasks[storageIndex];
        storedMasks.RemoveAt(storageIndex);
        return true;
    }
    
    /// <summary>
    /// Equip a specific mask directly.
    /// </summary>
    public void SetMask(Mask mask)
    {
        currentMask = mask;
    }
    
    /// <summary>
    /// Remove a mask from storage by index.
    /// </summary>
    public bool RemoveMaskFromStorage(int index)
    {
        if (index < 0 || index >= storedMasks.Count)
        {
            return false;
        }
        storedMasks.RemoveAt(index);
        return true;
    }
    
    /// <summary>
    /// Remove a specific mask from storage.
    /// </summary>
    public bool RemoveMaskFromStorage(Mask mask)
    {
        return storedMasks.Remove(mask);
    }
    
    /// <summary>
    /// Clear the currently worn mask.
    /// </summary>
    public void ClearMask()
    {
        currentMask = null;
    }
    
    // === Over-Time Effects ===
    
    public void SetBleed(float dps)
    {
        bleedDPS = Mathf.Max(0f, dps);
    }

    public void SetRegen(float hps)
    {
        regenHPS = Mathf.Max(0f, hps);
    }
    
    // === Loss Conditions ===
    
    private void OnStrengthDepleted()
    {
        Debug.LogWarning($"God strength depleted! LOSS CONDITION.");
        // TODO: Notify GameManager of loss
    }
    
    private void OnFavorDepleted()
    {
        Debug.LogWarning($"God favor depleted! LOSS CONDITION.");
        // TODO: Notify GameManager of loss
    }
}
