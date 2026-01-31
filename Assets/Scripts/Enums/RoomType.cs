/// <summary>
/// Types of rooms that can exist in a church.
/// </summary>
public enum RoomType
{
    /// <summary>Empty slot where a room can be built.</summary>
    Empty,
    /// <summary>Followers recover commitment. No decay. Hub for reassignment.</summary>
    Sanctuary,
    /// <summary>Generates God Strength when triggered. Followers decay commitment.</summary>
    Altar,
    /// <summary>Generates Favor when triggered. Followers do NOT decay commitment.</summary>
    Pews,
    /// <summary>Recruits a citizen from the Marketplace when triggered. Followers decay.</summary>
    Mission,
    /// <summary>Generates a new Mask when triggered. Followers decay commitment.</summary>
    RitualHall,
    /// <summary>Generates Architecture masks for building new rooms. Followers decay.</summary>
    Workshop
}
