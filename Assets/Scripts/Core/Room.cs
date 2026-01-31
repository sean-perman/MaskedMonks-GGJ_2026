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
    
    [Header("Damage Settings")]
    [SerializeField] protected float commitmentDamagePerLevel = 15f; // Commitment hit when damaged
    [SerializeField] protected float repairRatePerFollowerPerSecond = 0.1f; // Repair progress per follower per second
    [SerializeField] protected float repairThreshold = 1f; // Accumulated repair needed to fix one level
    
    [Header("Runtime State")]
    [SerializeField] protected float clock = 0f;
    [SerializeField] protected List<Follower> followers = new();
    [SerializeField] protected float repairProgress = 0f; // Accumulator for repair
    
    /// <summary>Reference to the church this room belongs to.</summary>
    protected Church church;
    
    /// <summary>Reference to the cult that owns this church.</summary>
    protected Cult cult;
    
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
    /// Capacity equals Level (damage doesn't reduce capacity, but damaged slots need repair).
    /// </summary>
    public int Capacity => level;
    
    /// <summary>
    /// Number of undamaged slots (functional capacity).
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
    /// Each follower contributes 1 progress per second.
    /// </summary>
    protected virtual void AccumulateClock()
    {
        clock += followers.Count * Time.deltaTime;
        
        if (clock >= duration)
        {
            OnClockTrigger();
            clock = 0f;
        }
    }
    
    /// <summary>
    /// Called when the clock reaches the duration threshold.
    /// Override in subclasses to implement room-specific effects.
    /// </summary>
    protected abstract void OnClockTrigger();
    
    // === Repair System ===
    
    /// <summary>
    /// Accumulate repair progress based on follower count.
    /// When enough progress is accumulated, repair one level of damage.
    /// </summary>
    protected virtual void AccumulateRepair()
    {
        if (damage <= 0) return;
        
        repairProgress += followers.Count * repairRatePerFollowerPerSecond * Time.deltaTime;
        
        if (repairProgress >= repairThreshold)
        {
            RepairDamage(1);
            repairProgress = 0f;
            Debug.Log($"{type} at {location} repaired! Damage now: {damage}");
        }
    }
    
    /// <summary>
    /// Get the current repair progress as a percentage (0-1).
    /// </summary>
    public float RepairProgress => damage > 0 ? Mathf.Clamp01(repairProgress / repairThreshold) : 0f;
    
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
    /// </summary>
    public virtual void TakeDamage(int amount = 1)
    {
        int previousDamage = damage;
        damage += amount;
        
        // Cap damage at level (can't have more damage than levels)
        damage = Mathf.Min(damage, level);
        
        // Apply commitment damage to all followers in the room
        // Scales with the amount of damage taken
        float commitmentHit = commitmentDamagePerLevel * amount;
        foreach (var follower in followers)
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
