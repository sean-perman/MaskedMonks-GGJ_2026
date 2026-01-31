using UnityEngine;

/// <summary>
/// A one-use ability that can be equipped by a god.
/// Masks have types, durations, shelf life, and costs.
/// </summary>
[System.Serializable]
public class Mask
{
    [Header("Mask Configuration")]
    [SerializeField] private MaskType type;
    [SerializeField] private MaskTargetType targetType;
    
    [Header("Timing")]
    [SerializeField] private float duration;      // How long the effect lasts (0 = instant)
    [SerializeField] private float shelfLife;     // Time remaining before decay in storage
    [SerializeField] private float maxShelfLife;  // Original shelf life for reference
    
    [Header("Cost")]
    [SerializeField] private int favorCost;
    [SerializeField] private int moneyCost;
    [SerializeField] private int followerSacrifice;  // Number of followers to sacrifice
    
    [Header("Effect Values")]
    [SerializeField] private int effectValue;  // Generic value for the effect (damage, healing, etc.)
    
    // === Properties ===
    
    public MaskType Type => type;
    public MaskTargetType TargetType => targetType;
    public float Duration => duration;
    public float ShelfLife => shelfLife;
    public float MaxShelfLife => maxShelfLife;
    public float ShelfLifePercent => maxShelfLife > 0 ? shelfLife / maxShelfLife : 0f;
    public int FavorCost => favorCost;
    public int MoneyCost => moneyCost;
    public int FollowerSacrifice => followerSacrifice;
    public int EffectValue => effectValue;
    public bool IsExpired => shelfLife <= 0f;
    public bool IsInstant => duration <= 0f;
    
    // === Constructors ===
    
    public Mask(MaskType type, MaskTargetType targetType, float duration, float shelfLife, 
                int favorCost, int moneyCost = 0, int followerSacrifice = 0, int effectValue = 10)
    {
        this.type = type;
        this.targetType = targetType;
        this.duration = duration;
        this.shelfLife = shelfLife;
        this.maxShelfLife = shelfLife;
        this.favorCost = favorCost;
        this.moneyCost = moneyCost;
        this.followerSacrifice = followerSacrifice;
        this.effectValue = effectValue;
    }
    
    // === Shelf Life ===
    
    /// <summary>
    /// Tick down the shelf life while in storage.
    /// </summary>
    public void TickShelfLife(float deltaTime)
    {
        if (shelfLife > 0f)
        {
            shelfLife = Mathf.Max(0f, shelfLife - deltaTime);
        }
    }
    
    // === Cost Checking ===
    
    /// <summary>
    /// Check if a cult can afford to activate this mask.
    /// </summary>
    public bool CanAfford(Cult cult)
    {
        if (cult == null || cult.god == null) return false;
        
        // Check favor
        if (!cult.god.CanAffordFavor(favorCost)) return false;
        
        // Check money
        if (cult.Money < moneyCost) return false;
        
        // Check followers for sacrifice
        if (cult.FollowerCount < followerSacrifice) return false;
        
        return true;
    }
    
    /// <summary>
    /// Deduct the cost from the cult (call after CanAfford returns true).
    /// </summary>
    public void PayCost(Cult cult)
    {
        if (cult == null || cult.god == null) return;
        
        cult.god.DecreaseFavor(favorCost);
        cult.SpendMoney(moneyCost);
        
        // TODO: Handle follower sacrifice (remove random followers)
        // for (int i = 0; i < followerSacrifice; i++) { ... }
    }
    
    // === Effect Application ===
    
    /// <summary>
    /// Apply this mask's effect. Call this after paying the cost.
    /// </summary>
    /// <param name="sourceCult">The cult activating the mask.</param>
    /// <param name="targetRoom">The target room (if applicable).</param>
    /// <param name="targetGod">The target god (if applicable).</param>
    public void ApplyEffect(Cult sourceCult, Room targetRoom = null, God targetGod = null)
    {
        switch (type)
        {
            case MaskType.Smiting:
                // Damage followers in target enemy room
                if (targetRoom != null)
                {
                    foreach (var follower in targetRoom.Followers)
                    {
                        follower.DecayCommitment(effectValue);
                    }
                    Debug.Log($"Smiting applied to room! {targetRoom.Followers.Count} followers affected.");
                }
                break;
                
            case MaskType.Wrath:
                // Direct damage to enemy god strength
                if (targetGod != null)
                {
                    targetGod.DecreaseStrength(effectValue);
                    Debug.Log($"Wrath dealt {effectValue} damage to enemy god!");
                }
                break;
                
            case MaskType.Whispers:
                // Reduce commitment in target enemy room
                if (targetRoom != null)
                {
                    foreach (var follower in targetRoom.Followers)
                    {
                        follower.DecayCommitment(effectValue);
                    }
                    Debug.Log($"Whispers reduced commitment in enemy room!");
                }
                break;
                
            case MaskType.Sanctuary:
                // Boost commitment in target own room
                if (targetRoom != null)
                {
                    foreach (var follower in targetRoom.Followers)
                    {
                        follower.RecoverCommitment(effectValue);
                    }
                    Debug.Log($"Sanctuary mask boosted commitment in own room!");
                }
                break;
                
            case MaskType.Plenty:
                // Instant favor gain
                if (sourceCult?.god != null)
                {
                    sourceCult.god.IncreaseFavor(effectValue);
                    Debug.Log($"Plenty granted {effectValue} favor!");
                }
                break;
                
            case MaskType.Sacrifice:
                // Sacrifice follower to heal god strength (follower sacrifice handled in PayCost)
                if (sourceCult?.god != null)
                {
                    sourceCult.god.IncreaseStrength(effectValue);
                    Debug.Log($"Sacrifice healed god for {effectValue} strength!");
                }
                break;
                
            // Architecture masks - build rooms
            case MaskType.ArchitectSanctuary:
            case MaskType.ArchitectAltar:
            case MaskType.ArchitectPews:
            case MaskType.ArchitectMission:
            case MaskType.ArchitectRitualHall:
            case MaskType.ArchitectWorkshop:
                // TODO: Implement room building
                Debug.Log($"Architecture mask {type} - room building not yet implemented.");
                break;
                
            default:
                Debug.LogWarning($"Unknown mask type: {type}");
                break;
        }
    }
}
