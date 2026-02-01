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
    
    // === Architecture Masks (DEPRECATED - room building removed) ===
    /// <summary>DEPRECATED: Build a Sanctuary room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectSanctuary,
    /// <summary>DEPRECATED: Build an Altar room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectAltar,
    /// <summary>DEPRECATED: Build a Pews room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectPews,
    /// <summary>DEPRECATED: Build a Mission room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectMission,
    /// <summary>DEPRECATED: Build a Ritual Hall room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectRitualHall,
    /// <summary>DEPRECATED: Build a Workshop room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectWorkshop,
    /// <summary>DEPRECATED: Build a Fundraising room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectFundraising,
    /// <summary>DEPRECATED: Build a Lightning Ritual room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectLightningRitual,
    /// <summary>DEPRECATED: Build a Flood Ritual room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectFloodRitual,
    /// <summary>DEPRECATED: Build a Shield Ritual room.</summary>
    [System.Obsolete("Architecture masks are deprecated. Room building has been removed.")]
    ArchitectShieldRitual
}
