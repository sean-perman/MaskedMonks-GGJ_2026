using UnityEngine;

/// <summary>
/// Shield Ritual room - Generates Shield masks that auto-block incoming attacks.
/// Shield masks cannot be manually activated. When you are attacked while holding
/// a Shield mask and have 4+ favor, the shield consumes 4 favor and negates the attack.
/// Generates every 35 pawn-seconds, has 8 second shelf life.
/// Followers decay commitment while working here.
/// </summary>
public class ShieldRitualRoom : Room
{
    private int ShieldFavorCost => GameConfig.Instance.shieldFavorCost;
    private float MaskShelfLife => GameConfig.Instance.shieldMaskShelfLife;
    
    public override ResourceType GeneratedResource => ResourceType.Mask;
    
    protected override void Awake()
    {
        type = RoomType.ShieldRitual;
        duration = GameConfig.Instance.shieldRitualDuration;
    }
    
    /// <summary>
    /// When the shield ritual triggers, generate a new Shield mask.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult == null || cult.god == null) return;
        
        // Check if god has storage space
        if (cult.god.MaskStorageRemaining <= 0)
        {
            Debug.Log("Shield Ritual triggered but god has no mask storage space!");
            return;
        }
        
        // Create the Shield mask
        var mask = new Mask(
            type: MaskType.Shield,
            targetType: MaskTargetType.Passive,
            duration: 0f, // Not applicable - passive effect
            shelfLife: MaskShelfLife,
            favorCost: ShieldFavorCost, // Cost when auto-activated
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: 1 // Blocks 1 attack
        );
        
        // Add to god's storage
        bool added = cult.god.AddMaskToStorage(mask);
        if (added)
        {
            AudioManager.PlayRoomTriggerRitual();
            NotifyResourceGenerated(ResourceType.Mask, 1);
            NotifyMaskGenerated(MaskType.Shield);
            Debug.Log($"Shield Ritual generated a Shield mask! (Auto-activates on attack if {ShieldFavorCost}+ favor)");
        }
    }
}
