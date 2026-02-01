using UnityEngine;

/// <summary>
/// Flood Ritual room - Generates Flood masks that hit the bottom row.
/// Flood masks deal 2 damage to every room in the opponent's bottom row.
/// Generates every 60 pawn-seconds, costs 2 favor, has 5 second shelf life.
/// Followers decay commitment while working here.
/// </summary>
public class FloodRitualRoom : Room
{
    [Header("Flood Ritual Settings")]
    [Tooltip("Duration for mask generation (pawn-seconds)")]
    [SerializeField] private float triggerDuration = 60f;
    
    [Tooltip("Favor cost for Flood masks")]
    [SerializeField] private int maskFavorCost = 2;
    
    [Tooltip("Damage dealt to each room in bottom row")]
    [SerializeField] private int damagePerRoom = 2;
    
    [Tooltip("Shelf life for generated masks in seconds")]
    [SerializeField] private float maskShelfLife = 5f;
    
    public override ResourceType GeneratedResource => ResourceType.Mask;
    
    protected override void Awake()
    {
        type = RoomType.FloodRitual;
        duration = triggerDuration;
    }
    
    /// <summary>
    /// When the flood ritual triggers, generate a new Flood mask.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult == null || cult.god == null) return;
        
        // Check if god has storage space
        if (cult.god.MaskStorageRemaining <= 0)
        {
            Debug.Log("Flood Ritual triggered but god has no mask storage space!");
            return;
        }
        
        // Create the Flood mask
        var mask = new Mask(
            type: MaskType.Flood,
            targetType: MaskTargetType.EnemyBottomRow,
            duration: 0f, // Instant effect
            shelfLife: maskShelfLife,
            favorCost: maskFavorCost,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: damagePerRoom
        );
        
        // Add to god's storage
        bool added = cult.god.AddMaskToStorage(mask);
        if (added)
        {
            NotifyResourceGenerated(ResourceType.Mask, 1);
            NotifyMaskGenerated(MaskType.Flood);
            Debug.Log($"Flood Ritual generated a Flood mask! (Cost: {maskFavorCost} favor, Damage: {damagePerRoom} per room)");
        }
    }
}
