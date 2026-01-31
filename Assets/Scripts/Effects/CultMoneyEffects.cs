using UnityEngine;

/// <summary>
/// Effect that generates money for a cult.
/// </summary>
[CreateAssetMenu(fileName = "GenerateMoney", menuName = "Effects/Cult/Generate Money")]
public class GenerateMoneyEffect : GameEffect
{
    [Header("Money Settings")]
    public int amount = 100;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetCult == null)
        {
            Debug.LogWarning($"GenerateMoneyEffect '{effectName}': No target cult in context!");
            return;
        }
        GameActions.GenerateMoney(context.targetCult, amount);
    }
}

/// <summary>
/// Effect that decreases money from a cult.
/// </summary>
[CreateAssetMenu(fileName = "DecreaseMoney", menuName = "Effects/Cult/Decrease Money")]
public class DecreaseMoneyEffect : GameEffect
{
    [Header("Money Settings")]
    public int amount = 100;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetCult == null)
        {
            Debug.LogWarning($"DecreaseMoneyEffect '{effectName}': No target cult in context!");
            return;
        }
        GameActions.DecreaseMoney(context.targetCult, amount);
    }
}
