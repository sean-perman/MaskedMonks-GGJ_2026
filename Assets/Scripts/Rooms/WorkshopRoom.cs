using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Workshop room - Generates Architecture masks for building new rooms.
/// Produces masks for room types the cult doesn't yet have.
/// Each room type has an intrinsic gold cost.
/// Takes 60 seconds of pawn-time to generate a blueprint.
/// Followers decay commitment while working here.
/// </summary>
public class WorkshopRoom : Room
{
    public override ResourceType GeneratedResource => ResourceType.Blueprint;
    
    protected override void Awake()
    {
        type = RoomType.Workshop;
        duration = GameConfig.Instance.workshopBlueprintDuration;
    }
    
    /// <summary>
    /// When the workshop triggers, generate an architecture mask for a room type
    /// that the cult doesn't already have.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult == null || cult.god == null || cult.church == null)
        {
            Debug.LogWarning("Workshop cannot generate blueprint - missing cult/god/church reference!");
            return;
        }
        
        // Get list of room types we don't have yet
        var missingRoomTypes = GetMissingRoomTypes();
        
        if (missingRoomTypes.Count == 0)
        {
            Debug.Log("Workshop triggered but cult has all room types! No blueprint generated.");
            return;
        }
        
        // Pick a random missing room type
        var targetRoomType = missingRoomTypes[Random.Range(0, missingRoomTypes.Count)];
        var maskType = RoomTypeToArchitectMask(targetRoomType);
        var goldCost = GetRoomCost(targetRoomType);
        
        // Create the architecture mask
        var blueprintMask = new Mask(
            type: maskType,
            targetType: MaskTargetType.OwnEmptySlot,
            duration: 0f, // Instant effect
            shelfLife: 120f, // 2 minutes shelf life
            favorCost: 0, // No favor cost
            moneyCost: goldCost, // Gold cost based on room type
            followerSacrifice: 0,
            effectValue: 0
        );
        
        // Try to add to god's storage
        bool added = cult.god.AddMaskToStorage(blueprintMask);
        
        if (added)
        {
            NotifyResourceGenerated(ResourceType.Blueprint, 1);
            NotifyMaskGenerated(maskType); // Notify for visual indicator
            Debug.Log($"Workshop generated a {targetRoomType} blueprint! Cost: {goldCost} gold");
        }
        else
        {
            Debug.LogWarning($"Workshop generated {targetRoomType} blueprint but god's mask storage is full!");
        }
    }
    
    /// <summary>
    /// Get list of room types the cult doesn't have yet.
    /// </summary>
    private List<RoomType> GetMissingRoomTypes()
    {
        var missing = new List<RoomType>();
        
        // Check each buildable room type
        RoomType[] buildableTypes = {
            RoomType.Sanctuary,
            RoomType.Altar,
            RoomType.Pews,
            RoomType.Mission,
            RoomType.WrathRitualHall,
            RoomType.Workshop,
            RoomType.Fundraising,
            RoomType.LightningRitual,
            RoomType.FloodRitual,
            RoomType.ShieldRitual
        };
        
        foreach (var roomType in buildableTypes)
        {
            if (cult.church.GetRoomOfType(roomType) == null)
            {
                missing.Add(roomType);
            }
        }
        
        return missing;
    }
    
    /// <summary>
    /// Convert a RoomType to its corresponding Architecture MaskType.
    /// </summary>
    private MaskType RoomTypeToArchitectMask(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Sanctuary => MaskType.ArchitectSanctuary,
            RoomType.Altar => MaskType.ArchitectAltar,
            RoomType.Pews => MaskType.ArchitectPews,
            RoomType.Mission => MaskType.ArchitectMission,
            RoomType.WrathRitualHall => MaskType.ArchitectRitualHall,
            RoomType.Workshop => MaskType.ArchitectWorkshop,
            RoomType.Fundraising => MaskType.ArchitectFundraising,
            RoomType.LightningRitual => MaskType.ArchitectLightningRitual,
            RoomType.FloodRitual => MaskType.ArchitectFloodRitual,
            RoomType.ShieldRitual => MaskType.ArchitectShieldRitual,
            _ => MaskType.ArchitectSanctuary // Default fallback
        };
    }
    
    /// <summary>
    /// Get the gold cost for building a specific room type.
    /// </summary>
    private int GetRoomCost(RoomType roomType)
    {
        return GameConfig.Instance.GetRoomBuildCost(roomType);
    }
    
    /// <summary>
    /// Get the gold cost for a room type (static version for external use).
    /// </summary>
    public static int GetRoomCostStatic(RoomType roomType)
    {
        return GameConfig.Instance.GetRoomBuildCost(roomType);
    }
}
