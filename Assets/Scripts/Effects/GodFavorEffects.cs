using UnityEngine;

/// <summary>
/// Effect that lowers a god's favor.
/// </summary>
[CreateAssetMenu(fileName = "LowerFavor", menuName = "Effects/God/Lower Favor")]
public class LowerFavorEffect : GameEffect
{
    [Header("Favor Settings")]
    public int amount = 10;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetGod == null)
        {
            Debug.LogWarning($"LowerFavorEffect '{effectName}': No target god in context!");
            return;
        }
        GameActions.LowerFavor(context.targetGod, amount);
    }
}

/// <summary>
/// Effect that raises a god's favor.
/// </summary>
[CreateAssetMenu(fileName = "RaiseFavor", menuName = "Effects/God/Raise Favor")]
public class RaiseFavorEffect : GameEffect
{
    [Header("Favor Settings")]
    public int amount = 10;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetGod == null)
        {
            Debug.LogWarning($"RaiseFavorEffect '{effectName}': No target god in context!");
            return;
        }
        GameActions.RaiseFavor(context.targetGod, amount);
    }
}
