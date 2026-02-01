using UnityEngine;

/// <summary>
/// Lightning Ritual room - Generates Lightning masks that hit an entire column.
/// Lightning masks deal 1 damage to every room in a column.
/// Generates every 25 pawn-seconds, costs 3 favor, has 30 second shelf life.
/// Followers decay commitment while working here.
/// </summary>
public class LightningRitualRoom : Room
{
    private int MaskFavorCost => GameConfig.Instance.lightningMaskFavorCost;
    private int DamagePerRoom => GameConfig.Instance.lightningDamagePerRoom;
    private float MaskShelfLife => GameConfig.Instance.lightningMaskShelfLife;
    
    public override ResourceType GeneratedResource => ResourceType.Mask;
    
    protected override void Awake()
    {
        type = RoomType.LightningRitual;
        duration = GameConfig.Instance.lightningRitualDuration;
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
            shelfLife: MaskShelfLife,
            favorCost: MaskFavorCost,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: DamagePerRoom
        );
        
        // Add to god's storage
        bool added = cult.god.AddMaskToStorage(mask);
        if (added)
        {
            AudioManager.PlayRoomTriggerRitual();
            NotifyResourceGenerated(ResourceType.Mask, 1);
            NotifyMaskGenerated(MaskType.Lightning);
            Debug.Log($"Lightning Ritual generated a Lightning mask! (Cost: {MaskFavorCost} favor, Damage: {DamagePerRoom} per room)");
        }
    }
}
