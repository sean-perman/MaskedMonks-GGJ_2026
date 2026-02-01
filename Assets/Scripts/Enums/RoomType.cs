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
    /// <summary>Generates Strike masks when triggered. Followers decay commitment.</summary>
    WrathRitualHall,
    /// <summary>Generates Architecture masks for building new rooms. Followers decay.</summary>
    Workshop,
    /// <summary>Generates money at the expense of favor. Followers decay commitment.</summary>
    Fundraising,
    /// <summary>Generates Lightning masks that hit an entire column. Followers decay.</summary>
    LightningRitual,
    /// <summary>Generates Flood masks that hit bottom row. Followers decay.</summary>
    FloodRitual,
    /// <summary>Generates Shield masks that auto-block attacks. Followers decay.</summary>
    ShieldRitual,
    /// <summary>Sacrifices a follower to deal direct damage to enemy god.</summary>
    SacrificialAltar
}
