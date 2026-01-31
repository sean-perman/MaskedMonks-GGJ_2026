using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An effect that applies multiple other effects at once.
/// Useful for creating complex effect combinations as a single asset.
/// </summary>
[CreateAssetMenu(fileName = "CompositeEffect", menuName = "Effects/Composite Effect")]
public class CompositeEffect : GameEffect
{
    [Header("Child Effects")]
    [Tooltip("All effects in this list will be applied when this effect is triggered")]
    public List<GameEffect> childEffects = new();
    
    public override void Apply(EffectContext context)
    {
        foreach (var effect in childEffects)
        {
            if (effect != null)
            {
                effect.Apply(context);
            }
        }
    }
}
