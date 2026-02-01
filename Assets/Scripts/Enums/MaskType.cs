/// <summary>
/// Types of masks that can be generated and used.
/// </summary>
public enum MaskType
{
    // === Combat Masks ===
    /// <summary>Deal damage to a targeted enemy room.</summary>
    Strike,
    /// <summary>Damage followers in target enemy room.</summary>
    Smiting,
    /// <summary>Direct damage to enemy god strength.</summary>
    Wrath,
    /// <summary>Reduce commitment in target enemy room.</summary>
    Whispers,
    /// <summary>Deal 1 damage to all rooms in a column.</summary>
    Lightning,
    /// <summary>Deal 2 damage to all rooms in bottom row.</summary>
    Flood,
    
    // === Support Masks ===
    /// <summary>Boost commitment in target own room.</summary>
    Sanctuary,
    /// <summary>Instant favor gain.</summary>
    Plenty,
    /// <summary>Sacrifice follower to heal god strength.</summary>
    Sacrifice,
    /// <summary>Auto-blocks incoming damage if you have 4+ favor.</summary>
    Shield,
    
    // === Architecture Masks (for building rooms) ===
    /// <summary>Build a Sanctuary room.</summary>
    ArchitectSanctuary,
    /// <summary>Build an Altar room.</summary>
    ArchitectAltar,
    /// <summary>Build a Pews room.</summary>
    ArchitectPews,
    /// <summary>Build a Mission room.</summary>
    ArchitectMission,
    /// <summary>Build a Ritual Hall room.</summary>
    ArchitectRitualHall,
    /// <summary>Build a Workshop room.</summary>
    ArchitectWorkshop,
    /// <summary>Build a Fundraising room.</summary>
    ArchitectFundraising
}
