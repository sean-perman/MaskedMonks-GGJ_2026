using UnityEngine;

/// <summary>
/// Lightning Ritual room - Generates Lightning masks that hit an entire column.
/// Lightning masks deal 1 damage to every room in a column.
/// Generates every 25 pawn-seconds, costs 3 favor, has 30 second shelf life.
/// Followers decay commitment while working here.
/// </summary>
public class LightningRitualRoom : Room
{
    [Header("Lightning Ritual Settings")]
    [Tooltip("Duration for mask generation (pawn-seconds)")]
    [SerializeField] private float triggerDuration = 25f;
    
    [Tooltip("Favor cost for Lightning masks")]
    [SerializeField] private int maskFavorCost = 3;
    
    [Tooltip("Damage dealt to each room in the column")]
    [SerializeField] private int damagePerRoom = 1;
    
    [Tooltip("Shelf life for generated masks in seconds")]
    [SerializeField] private float maskShelfLife = 30f;
    
    [Tooltip("Cooldown between mask uses in seconds")]
    [SerializeField] private float maskCooldown = 20f;
    
    public override ResourceType GeneratedResource => ResourceType.Mask;
    
    protected override void Awake()
    {
        type = RoomType.LightningRitual;
        duration = triggerDuration;
    }
    
    /// <summary>
    /// When the lightning ritual triggers, generate a new Lightning mask.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult == null || cult.god == null) return;
        
        // Check if god has storage space
        if (cult.god.MaskStorageRemaining <= 0)
        {
            Debug.Log("Lightning Ritual triggered but god has no mask storage space!");
            return;
        }
        
        // Create the Lightning mask
        var mask = new Mask(
            type: MaskType.Lightning,
            targetType: MaskTargetType.EnemyColumn,
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
            NotifyMaskGenerated(MaskType.Lightning);
            Debug.Log($"Lightning Ritual generated a Lightning mask! (Cost: {maskFavorCost} favor, Damage: {damagePerRoom} per room)");
        }
    }
}
