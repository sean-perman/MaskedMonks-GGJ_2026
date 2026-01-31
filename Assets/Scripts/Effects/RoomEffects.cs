using UnityEngine;

/// <summary>
/// Effect that repairs a room's damage level.
/// </summary>
[CreateAssetMenu(fileName = "FixRoom", menuName = "Effects/Room/Fix Room")]
public class FixRoomEffect : GameEffect
{
    [Header("Fix Settings")]
    public int repairAmount = 1;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetRoom == null)
        {
            Debug.LogWarning($"FixRoomEffect '{effectName}': No target room in context!");
            return;
        }
        GameActions.FixRoom(context.targetRoom, repairAmount);
    }
}

/// <summary>
/// Effect that damages a room's damage level.
/// </summary>
[CreateAssetMenu(fileName = "DamageRoom", menuName = "Effects/Room/Damage Room")]
public class DamageRoomEffect : GameEffect
{
    [Header("Damage Settings")]
    public int damageAmount = 1;
    
    public override void Apply(EffectContext context)
    {
        if (context.targetRoom == null)
        {
            Debug.LogWarning($"DamageRoomEffect '{effectName}': No target room in context!");
            return;
        }
        GameActions.DamageRoom(context.targetRoom, damageAmount);
    }
}
