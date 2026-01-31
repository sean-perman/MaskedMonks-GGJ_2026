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
    
    [Tooltip("Favor cost for generated masks")]
    [SerializeField] private int maskFavorCost = 2;
    
    [Tooltip("Effect value for generated masks (damage, etc.)")]
    [SerializeField] private int maskEffectValue = 2;
    
    [Tooltip("Shelf life for generated masks in seconds")]
    [SerializeField] private float maskShelfLife = 60f;
    
    [Tooltip("Duration for mask triggers (pawn-seconds)")]
    [SerializeField] private float triggerDuration = 15f;
    
    protected override void Awake()
    {
        type = RoomType.RitualHall;
        duration = triggerDuration;
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
            shelfLife: maskShelfLife,
            favorCost: maskFavorCost,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: maskEffectValue
        );
        
        // Add to god's storage
        bool added = cult.god.AddMaskToStorage(mask);
        if (added)
        {
            Debug.Log($"Ritual Hall generated a {maskType} mask! (Cost: {maskFavorCost} favor, Effect: {maskEffectValue})");
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
