using UnityEngine;

/// <summary>
/// Effect that instantly damages a god.
/// </summary>
[CreateAssetMenu(fileName = "InjureGod", menuName = "Effects/God/Injure God")]
public class InjureGodEffect : GameEffect
{
    [Header("Damage Settings")]
    public int damageAmount = 10;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetGod == null)
        {
            Debug.LogWarning($"InjureGodEffect '{effectName}': No target god in context!");
            return;
        }
        GameActions.InjureGod(context.targetGod, damageAmount);
    }
}

/// <summary>
/// Effect that applies bleeding (damage over time) to a god.
/// </summary>
[CreateAssetMenu(fileName = "BleedGod", menuName = "Effects/God/Bleed God")]
public class BleedGodEffect : GameEffect
{
    [Header("Bleed Settings")]
    public float damagePerSecond = 5f;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetGod == null)
        {
            Debug.LogWarning($"BleedGodEffect '{effectName}': No target god in context!");
            return;
        }
        GameActions.BleedGod(context.targetGod, damagePerSecond);
    }
}

/// <summary>
/// Effect that instantly heals a god.
/// </summary>
[CreateAssetMenu(fileName = "HealGod", menuName = "Effects/God/Heal God")]
public class HealGodEffect : GameEffect
{
    [Header("Heal Settings")]
    public int healAmount = 10;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetGod == null)
        {
            Debug.LogWarning($"HealGodEffect '{effectName}': No target god in context!");
            return;
        }
        GameActions.HealGod(context.targetGod, healAmount);
    }
}

/// <summary>
/// Effect that applies regeneration (healing over time) to a god.
/// </summary>
[CreateAssetMenu(fileName = "RegenGod", menuName = "Effects/God/Regen God")]
public class RegenGodEffect : GameEffect
{
    [Header("Regen Settings")]
    public float healthPerSecond = 5f;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetGod == null)
        {
            Debug.LogWarning($"RegenGodEffect '{effectName}': No target god in context!");
            return;
        }
        GameActions.RegenGod(context.targetGod, healthPerSecond);
    }
}
