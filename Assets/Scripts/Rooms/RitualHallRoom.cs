using UnityEngine;

/// <summary>
/// Ritual Hall room - Generates offensive masks when triggered.
/// Default: generates Strike masks (2 favor cost, 2 room damage) every 15 seconds.
/// Followers decay commitment while working here.
/// </summary>
public class RitualHallRoom : Room
{
    [Header("Ritual Hall Settings")]
    [Tooltip("The types of masks this ritual hall can generate")]
    [SerializeField] private MaskType[] possibleMasks = new MaskType[]
    {
        MaskType.Strike
    };
    
    private int MaskFavorCost => GameConfig.Instance.ritualHallMaskFavorCost;
    private int MaskEffectValue => GameConfig.Instance.ritualHallMaskEffectValue;
    private float MaskShelfLife => GameConfig.Instance.ritualHallMaskShelfLife;
    
    public override ResourceType GeneratedResource => ResourceType.Mask;
    
    protected override void Awake()
    {
        type = RoomType.WrathRitualHall;
        duration = GameConfig.Instance.ritualHallDuration;
    }
    
    /// <summary>
    /// When the ritual hall triggers, generate a new mask for the god.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult == null || cult.god == null) return;
        
        // Check if god has storage space
        if (cult.god.MaskStorageRemaining <= 0)
        {
            Debug.Log($"Ritual Hall triggered but god has no mask storage space!");
            return;
        }
        
        // Pick a random mask type from possible masks
        if (possibleMasks.Length == 0) return;
        
        var maskType = possibleMasks[Random.Range(0, possibleMasks.Length)];
        
        // Create the mask
        var mask = new Mask(
            type: maskType,
            targetType: GetTargetTypeForMask(maskType),
            duration: 0f, // Instant effect
            shelfLife: MaskShelfLife,
            favorCost: MaskFavorCost,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: MaskEffectValue
        );
        
        // Add to god's storage
        bool added = cult.god.AddMaskToStorage(mask);
        if (added)
        {
            NotifyResourceGenerated(ResourceType.Mask, 1);
            NotifyMaskGenerated(maskType);
            Debug.Log($"Ritual Hall generated a {maskType} mask! (Cost: {MaskFavorCost} favor, Effect: {MaskEffectValue})");
        }
    }
    
    /// <summary>
    /// Get the appropriate target type for a mask type.
    /// </summary>
    private MaskTargetType GetTargetTypeForMask(MaskType maskType)
    {
        return maskType switch
        {
            MaskType.Strike => MaskTargetType.EnemyRoom,
            MaskType.Lightning => MaskTargetType.EnemyColumn,
            MaskType.Flood => MaskTargetType.EnemyBottomRow,
            MaskType.Shield => MaskTargetType.Passive,
            MaskType.Smiting => MaskTargetType.EnemyRoom,
            MaskType.Wrath => MaskTargetType.EnemyRoom, // Target room, affects god
            MaskType.Whispers => MaskTargetType.EnemyRoom,
            MaskType.Sanctuary => MaskTargetType.OwnRoom,
            MaskType.Plenty => MaskTargetType.None,
            MaskType.Sacrifice => MaskTargetType.None,
            _ => MaskTargetType.None
        };
    }
}
