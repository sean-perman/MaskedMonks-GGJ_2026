using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that can be attached to any GameObject (like a mask) to apply effects.
/// Drag and drop effect ScriptableObjects into the effects list in the inspector.
/// </summary>
public class EffectApplier : MonoBehaviour
{
    [Header("Effects to Apply")]
    [Tooltip("List of effects that will be applied when TriggerEffects() is called")]
    public List<GameEffect> effects = new();
    
    [Header("Target References")]
    [Tooltip("If left empty, will try to find targets automatically")]
    public God targetGod;
    public Church targetRoom;
    public Cult targetCult;
    
    [Header("Auto-Find Settings")]
    [Tooltip("Automatically find targets from GameManager if references are empty")]
    public bool autoFindTargets = true;
    
    private EffectContext cachedContext;
    
    private void Start()
    {
        if (autoFindTargets)
        {
            TryAutoFindTargets();
        }
        BuildContext();
    }
    
    /// <summary>
    /// Attempt to find target references automatically.
    /// Override this in derived classes for custom target finding logic.
    /// </summary>
    protected virtual void TryAutoFindTargets()
    {
        if (targetGod == null)
            targetGod = FindFirstObjectByType<God>();
        if (targetRoom == null)
            targetRoom = FindFirstObjectByType<Church>();
    }
    
    private void BuildContext()
    {
        cachedContext = new EffectContext
        {
            targetGod = targetGod,
            targetRoom = targetRoom,
            targetCult = targetCult,
            source = gameObject
        };
    }
    
    /// <summary>
    /// Apply all effects in the list.
    /// Call this from mask behaviors, room triggers, etc.
    /// </summary>
    public void TriggerEffects()
    {
        if (cachedContext == null)
            BuildContext();
            
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                effect.Apply(cachedContext);
            }
        }
    }
    
    /// <summary>
    /// Apply all effects with a custom context.
    /// </summary>
    public void TriggerEffects(EffectContext context)
    {
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                effect.Apply(context);
            }
        }
    }
    
    /// <summary>
    /// Apply a single effect from the list by index.
    /// </summary>
    public void TriggerEffect(int index)
    {
        if (index >= 0 && index < effects.Count && effects[index] != null)
        {
            if (cachedContext == null)
                BuildContext();
            effects[index].Apply(cachedContext);
        }
    }
    
    /// <summary>
    /// Apply a specific effect (doesn't need to be in the list).
    /// </summary>
    public void TriggerEffect(GameEffect effect)
    {
        if (effect != null)
        {
            if (cachedContext == null)
                BuildContext();
            effect.Apply(cachedContext);
        }
    }
    
    /// <summary>
    /// Update the target references and rebuild the context.
    /// </summary>
    public void SetTargets(God god = null, Church room = null, Cult cult = null)
    {
        if (god != null) targetGod = god;
        if (room != null) targetRoom = room;
        if (cult != null) targetCult = cult;
        BuildContext();
    }
}
