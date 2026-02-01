/// <summary>
/// Types of resources that rooms can generate.
/// Used for visual feedback (progress bar colors, spawn icons).
/// </summary>
public enum ResourceType
{
    /// <summary>No resource generated (e.g., Sanctuary).</summary>
    None,
    /// <summary>God strength (Altar).</summary>
    Strength,
    /// <summary>God favor (Pews).</summary>
    Favor,
    /// <summary>Cult money (Fundraising).</summary>
    Money,
    /// <summary>Offensive masks (Ritual Hall).</summary>
    Mask,
    /// <summary>Architecture masks (Workshop).</summary>
    Blueprint,
    /// <summary>New followers (Mission).</summary>
    Follower
}
