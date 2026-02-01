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
    [Header("Workshop Settings")]
    [Tooltip("Base duration for generating a blueprint (in pawn-seconds)")]
    [SerializeField] private float blueprintDuration = 60f;
    
    [Header("Room Costs (Gold)")]
    [SerializeField] private int sanctuaryCost = 50;
    [SerializeField] private int altarCost = 80;
    [SerializeField] private int pewsCost = 60;
    [SerializeField] private int missionCost = 100;
    [SerializeField] private int ritualHallCost = 120;
    [SerializeField] private int workshopCost = 150;
    [SerializeField] private int fundraisingCost = 70;
    
    public override ResourceType GeneratedResource => ResourceType.Blueprint;
    
    protected override void Awake()
    {
        type = RoomType.Workshop;
        duration = blueprintDuration;
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
            RoomType.RitualHall,
            RoomType.Workshop,
            RoomType.Fundraising
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
            RoomType.RitualHall => MaskType.ArchitectRitualHall,
            RoomType.Workshop => MaskType.ArchitectWorkshop,
            RoomType.Fundraising => MaskType.ArchitectFundraising,
            _ => MaskType.ArchitectSanctuary // Default fallback
        };
    }
    
    /// <summary>
    /// Get the gold cost for building a specific room type.
    /// </summary>
    private int GetRoomCost(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Sanctuary => sanctuaryCost,
            RoomType.Altar => altarCost,
            RoomType.Pews => pewsCost,
            RoomType.Mission => missionCost,
            RoomType.RitualHall => ritualHallCost,
            RoomType.Workshop => workshopCost,
            RoomType.Fundraising => fundraisingCost,
            _ => 100 // Default cost
        };
    }
    
    /// <summary>
    /// Get the gold cost for a room type (static version for external use).
    /// </summary>
    public static int GetRoomCostStatic(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Sanctuary => 50,
            RoomType.Altar => 80,
            RoomType.Pews => 60,
            RoomType.Mission => 100,
            RoomType.RitualHall => 120,
            RoomType.Workshop => 150,
            RoomType.Fundraising => 70,
            _ => 100
        };
    }
}
