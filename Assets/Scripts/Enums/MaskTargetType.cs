/// <summary>
/// Targeting modes for masks.
/// </summary>
public enum MaskTargetType
{
    /// <summary>Global effect, no targeting needed.</summary>
    None,
    /// <summary>Must select an enemy room to target.</summary>
    EnemyRoom,
    /// <summary>Must select one of your own rooms to target.</summary>
    OwnRoom,
    /// <summary>Must select an empty slot in your own church.</summary>
    OwnEmptySlot,
    /// <summary>Must select an enemy column (left-right targeting only).</summary>
    EnemyColumn,
    /// <summary>Automatically targets bottom row of enemy church.</summary>
    EnemyBottomRow,
    /// <summary>Passive defensive mask - auto-activates when attacked.</summary>
    Passive
}
