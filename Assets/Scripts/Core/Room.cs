using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all room types in a church.
/// Rooms hold followers who work to accumulate progress toward triggering effects.
/// </summary>
public abstract class Room : MonoBehaviour
{
    [Header("Room Configuration")]
    [SerializeField] protected RoomType type;
    [SerializeField] protected Vector2Int location;
    
    [Header("Room Stats")]
    [SerializeField] protected int level = 1;
    [SerializeField] protected int damage = 0;
    [SerializeField] protected float duration = 10f; // Seconds of work needed to trigger
    
    [Header("Runtime State")]
    [SerializeField] protected float clock = 0f;
    [SerializeField] protected List<Follower> followers = new();
    [SerializeField] protected float repairProgress = 0f; // Accumulator for repair
    
    // Config properties
    protected float CommitmentDamagePerLevel => GameConfig.Instance.commitmentDamagePerLevel;
    protected float RepairRatePerFollowerPerSecond => GameConfig.Instance.repairRatePerFollowerPerSecond;
    protected float RepairThreshold => GameConfig.Instance.repairThreshold;
    protected int MaxRoomLevel => GameConfig.Instance.maxRoomLevel;
    
    /// <summary>Reference to the church this room belongs to.</summary>
    protected Church church;
    
    /// <summary>Reference to the cult that owns this church.</summary>
    protected Cult cult;
    
    // === Events ===
    
    /// <summary>
    /// Fired when this room generates a resource.
    /// Parameters: ResourceType, int amount
    /// </summary>
    public event Action<ResourceType, int> OnResourceGenerated;
    
    /// <summary>
    /// Fired when this room generates a mask specifically.
    /// Parameters: MaskType of generated mask
    /// </summary>
    public event Action<MaskType> OnMaskGenerated;
    
    // === Properties ===
    
    public RoomType Type => type;
    public Vector2Int Location => location;
    public int Level => level;
    public int Damage => damage;
    public float Duration => duration;
    public float Clock => clock;
    public IReadOnlyList<Follower> Followers => followers;
    
    /// <summary>
    /// Maximum number of followers this room can hold.
    /// Capacity = Level - RedDamage (red slots can't hold pawns, orange slots can).
    /// </summary>
    public int Capacity => Mathf.Max(0, level - RedDamage);
    
    /// <summary>
    /// Maximum damage this room can sustain (2 * Level - 1).
    /// At max damage: 1 orange slot + (Level - 1) red slots.
    /// </summary>
    public int MaxDamage => 2 * level - 1;
    
    /// <summary>
    /// Number of orange-damaged slots (damaged but still functional, pawns help repair).
    /// Orange damage fills first, then converts to red.
    /// </summary>
    public int OrangeDamage => Mathf.Min(level, damage);
    
    /// <summary>
    /// Number of red-damaged slots (completely non-functional, can't hold pawns).
    /// Red damage = damage beyond Level.
    /// </summary>
    public int RedDamage => Mathf.Max(0, damage - level);
    
    /// <summary>
    /// Number of completely undamaged slots.
    /// </summary>
    public int FunctionalCapacity => Mathf.Max(0, level - damage);
    
    /// <summary>
    /// Whether this room has space for more followers.
    /// </summary>
    public bool HasSpace => followers.Count < Capacity;
    
    /// <summary>
    /// Whether followers in this room lose commitment over time.
    /// Override in subclasses where decay should be disabled (Sanctuary, Pews).
    /// </summary>
    public virtual bool CausesCommitmentDecay => true;
    
    /// <summary>
    /// The type of resource this room generates.
    /// Override in subclasses to specify resource type for visual feedback.
    /// </summary>
    public virtual ResourceType GeneratedResource => ResourceType.None;
    
    /// <summary>
    /// Progress toward triggering the room effect (0 to 1).
    /// </summary>
    public float Progress => duration > 0 ? Mathf.Clamp01(clock / duration) : 0f;
    
    // === Unity Lifecycle ===
    
    protected virtual void Awake()
    {
        // Subclasses should set their type here
    }
    
    // === Initialization ===
    
    /// <summary>
    /// Initialize the room with references to its church and cult.
    /// </summary>
    public virtual void Initialize(Church church, Cult cult, Vector2Int location)
    {
        this.church = church;
        this.cult = cult;
        this.location = location;
    }
    
    // === Update Loop ===
    
    protected virtual void Update()
    {
        if (followers.Count > 0)
        {
            AccumulateClock();
            
            // If room is damaged, followers work to repair it
            if (damage > 0)
            {
                AccumulateRepair();
            }
        }
    }
    
    // === Clock System ===
    
    /// <summary>
    /// Accumulate progress on the room's clock based on follower count.
    /// Only followers in UNDAMAGED slots contribute to progress.
    /// Followers in orange-damaged slots only contribute to repairs.
    /// </summary>
    protected virtual void AccumulateClock()
    {
        // Only undamaged slots are productive
        int productiveFollowers = Mathf.Min(followers.Count, FunctionalCapacity);
        
        if (productiveFollowers > 0)
        {
            clock += productiveFollowers * Time.deltaTime;
            
            if (clock >= duration)
            {
                OnClockTrigger();
                clock = 0f;
            }
        }
    }
    
    /// <summary>
    /// Called when the clock reaches the duration threshold.
    /// Override in subclasses to implement room-specific effects.
    /// </summary>
    protected abstract void OnClockTrigger();
    
    /// <summary>
    /// Notify listeners that a resource was generated.
    /// Call this from subclass OnClockTrigger implementations.
    /// </summary>
    protected void NotifyResourceGenerated(ResourceType resource, int amount)
    {
        OnResourceGenerated?.Invoke(resource, amount);
    }
    
    /// <summary>
    /// Notify listeners that a mask was generated.
    /// Call this from ritual room OnClockTrigger implementations.
    /// </summary>
    protected void NotifyMaskGenerated(MaskType maskType)
    {
        OnMaskGenerated?.Invoke(maskType);
    }
    
    // === Repair System ===
    
    /// <summary>
    /// Accumulate repair progress based on followers in orange-damaged slots.
    /// All pawns in orange slots contribute to repairs.
    /// Red damage is repaired first, then orange damage.
    /// When enough progress is accumulated, repair one level of damage.
    /// </summary>
    protected virtual void AccumulateRepair()
    {
        if (damage <= 0) return;
        
        // Only pawns in orange-damaged slots contribute to repairs
        // Orange slots = slots beyond FunctionalCapacity but within Capacity
        int undamagedSlots = FunctionalCapacity;
        int pawnsInOrangeSlots = Mathf.Max(0, followers.Count - undamagedSlots);
        
        // All pawns contribute to repair (simplified - all pawns help)
        repairProgress += followers.Count * RepairRatePerFollowerPerSecond * Time.deltaTime;
        
        if (repairProgress >= RepairThreshold)
        {
            RepairDamage(1);
            repairProgress = 0f;
            Debug.Log($"{type} at {location} repaired! Damage now: {damage}");
        }
    }
    
    /// <summary>
    /// Get the current repair progress as a percentage (0-1).
    /// </summary>
    public float RepairProgress => damage > 0 ? Mathf.Clamp01(repairProgress / RepairThreshold) : 0f;
    
    // === Follower Management ===
    
    /// <summary>
    /// Attempt to add a follower to this room.
    /// </summary>
    /// <returns>True if follower was added, false if room is at capacity.</returns>
    public virtual bool AddFollower(Follower follower)
    {
        if (!HasSpace)
        {
            Debug.LogWarning($"Room {type} at {location} is at capacity!");
            return false;
        }
        
        if (followers.Contains(follower))
        {
            Debug.LogWarning($"Follower is already in this room!");
            return false;
        }
        
        followers.Add(follower);
        follower.SetRoom(this);
        return true;
    }
    
    /// <summary>
    /// Remove a follower from this room.
    /// </summary>
    public virtual bool RemoveFollower(Follower follower)
    {
        if (!followers.Contains(follower))
        {
            return false;
        }
        
        followers.Remove(follower);
        follower.SetRoom(null);
        return true;
    }
    
    // === Damage & Upgrades ===
    
    /// <summary>
    /// Apply damage to the room.
    /// Followers inside take a commitment hit that scales with damage.
    /// Followers can still occupy damaged slots to repair them.
    /// If the defending cult has a shield mask and enough favor, the attack is blocked.
    /// </summary>
    public virtual void TakeDamage(int amount = 1)
    {
        // Check for shield block first
        if (cult != null && cult.god != null && cult.god.TryBlockAttackWithShield(this))
        {
            Debug.Log($"Attack on {type} was blocked by a Shield mask!");
            return;
        }
        
        int previousDamage = damage;
        damage += amount;
        
        // Cap damage at MaxDamage (2 * Level - 1)
        damage = Mathf.Min(damage, MaxDamage);
        
        // If damage increased red count, kick out pawns that no longer fit
        while (followers.Count > Capacity && followers.Count > 0)
        {
            var kickedFollower = followers[followers.Count - 1];
            RemoveFollower(kickedFollower);
            Debug.Log($"Follower {kickedFollower.name} was kicked from {type} due to damage!");
            
            // Try to send to sanctuary first
            var sanctuary = church?.GetRoomOfType(RoomType.Sanctuary);
            if (sanctuary != null && sanctuary.HasSpace)
            {
                sanctuary.AddFollower(kickedFollower);
            }
            else
            {
                // No room in sanctuary - follower leaves to marketplace
                Debug.Log($"Follower {kickedFollower.name} left the cult (no space in sanctuary)!");
                cult?.RemoveFollower(kickedFollower);
                if (kickedFollower != null && kickedFollower.gameObject != null)
                {
                    Destroy(kickedFollower.gameObject);
                }
            }
        }
        
        // Apply commitment damage to all remaining followers in the room
        // Make a copy to avoid collection modified exception
        float commitmentHit = CommitmentDamagePerLevel * amount;
        var remainingFollowers = new List<Follower>(followers);
        foreach (var follower in remainingFollowers)
        {
            follower.DecayCommitment(commitmentHit);
            Debug.Log($"Follower in {type} took {commitmentHit} commitment damage from room damage!");
        }
        
        Debug.Log($"{type} at {location} took {amount} damage! Total damage: {damage}");
    }
    
    /// <summary>
    /// Repair damage to the room.
    /// </summary>
    public virtual void RepairDamage(int amount = 1)
    {
        damage = Mathf.Max(0, damage - amount);
        
        // Reset repair progress if fully repaired
        if (damage <= 0)
        {
            repairProgress = 0f;
        }
    }
    
    /// <summary>
    /// Upgrade the room's level, increasing capacity.
    /// </summary>
    public virtual void IncreaseLevel(int amount = 1)
    {
        level += amount;
    }
    
    /// <summary>
    /// Upgrade the room by one level. Alias for IncreaseLevel(1).
    /// </summary>
    public void Upgrade()
    {
        IncreaseLevel(1);
        Debug.Log($"{type} upgraded to level {level}");
    }
    
    /// <summary>
    /// Set the duration threshold for triggering the room effect.
    /// </summary>
    public virtual void SetDuration(float newDuration)
    {
        duration = Mathf.Max(0.1f, newDuration);
    }
}
