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
    /// For offensive masks, spawns a projectile and applies damage on impact.
    /// </summary>
    /// <param name="sourceCult">The cult activating the mask.</param>
    /// <param name="targetRoom">The target room (if applicable).</param>
    /// <param name="targetGod">The target god (if applicable).</param>
    /// <param name="targetColumn">The target column index (for Lightning masks).</param>
    /// <param name="targetChurch">The target church (for area effects).</param>
    /// <param name="sourcePosition">World position to launch projectile from (optional).</param>
    public void ApplyEffect(Cult sourceCult, Room targetRoom = null, God targetGod = null, int targetColumn = -1, Church targetChurch = null, Vector3? sourcePosition = null)
    {
        // Get source position from cult's god position if not provided
        Vector3 srcPos = sourcePosition ?? GetDefaultSourcePosition(sourceCult);
        
        switch (type)
        {
            case MaskType.Strike:
                // Deal damage to a targeted enemy room
                if (targetRoom != null)
                {
                    Vector3 targetPos = GetRoomWorldPosition(targetRoom);
                    int damage = effectValue;
                    MaskProjectile.Create(srcPos, targetPos, type, () => {
                        targetRoom.TakeDamage(damage);
                        Debug.Log($"Strike dealt {damage} damage to enemy room {targetRoom.Type}!");
                    });
                }
                break;
                
            case MaskType.Lightning:
                // Deal damage to all rooms in a column
                if (targetChurch != null && targetColumn >= 0)
                {
                    // Play lightning sound
                    AudioManager.PlayRoomLightning();

                    // Fire projectiles at each room in the column
                    for (int y = 0; y < targetChurch.GridHeight; y++)
                    {
                        var room = targetChurch.GetRoomAt(new Vector2Int(targetColumn, y));
                        if (room != null && room.Type != RoomType.Empty)
                        {
                            Vector3 targetPos = GetRoomWorldPosition(room);
                            int damage = effectValue;
                            Room targetRoomCapture = room;
                            // Stagger the projectiles slightly
                            float delay = y * 0.1f;
                            if (delay > 0)
                            {
                                // Use coroutine for delayed spawns - handled in separate method
                                SpawnDelayedProjectile(srcPos, targetPos, type, targetRoomCapture, damage, delay);
                            }
                            else
                            {
                                MaskProjectile.Create(srcPos, targetPos, type, () => {
                                    targetRoomCapture.TakeDamage(damage);
                                });
                            }
                        }
                    }
                    Debug.Log($"Lightning struck column {targetColumn}!");
                }
                break;
                
            case MaskType.Flood:
                // Deal damage to all rooms in bottom row
                if (targetChurch != null)
                {
                    int bottomRow = 0;
                    for (int x = 0; x < targetChurch.GridWidth; x++)
                    {
                        var room = targetChurch.GetRoomAt(new Vector2Int(x, bottomRow));
                        if (room != null && room.Type != RoomType.Empty)
                        {
                            Vector3 targetPos = GetRoomWorldPosition(room);
                            int damage = effectValue;
                            Room targetRoomCapture = room;
                            // Stagger the flood wave
                            float delay = x * 0.15f;
                            if (delay > 0)
                            {
                                SpawnDelayedProjectile(srcPos, targetPos, type, targetRoomCapture, damage, delay);
                            }
                            else
                            {
                                MaskProjectile.Create(srcPos, targetPos, type, () => {
                                    targetRoomCapture.TakeDamage(damage);
                                });
                            }
                        }
                    }
                    Debug.Log($"Flood wave launched!");
                }
                break;
                
            case MaskType.Shield:
                // Shield masks are passive and don't have an active effect
                // They are consumed automatically when TryBlockAttack is called
                Debug.Log("Shield mask cannot be manually activated!");
                break;
                
            case MaskType.Smiting:
                // Damage followers in target enemy room
                if (targetRoom != null)
                {
                    Vector3 targetPos = GetRoomWorldPosition(targetRoom);
                    var followers = targetRoom.Followers;
                    int damage = effectValue;
                    MaskProjectile.Create(srcPos, targetPos, type, () => {
                        foreach (var follower in followers)
                        {
                            follower.DecayCommitment(damage);
                        }
                        Debug.Log($"Smiting applied to room! {followers.Count} followers affected.");
                    });
                }
                break;
                
            case MaskType.Wrath:
                // Direct damage to enemy god strength
                if (targetGod != null)
                {
                    // Target god position (above the church)
                    Vector3 targetPos = srcPos + Vector3.right * 20f + Vector3.up * 5f; // Approximate
                    int damage = effectValue;
                    God targetGodCapture = targetGod;
                    MaskProjectile.Create(srcPos, targetPos, type, () => {
                        targetGodCapture.DecreaseStrength(damage);
                        Debug.Log($"Wrath dealt {damage} damage to enemy god!");
                    });
                }
                break;
                
            case MaskType.Whispers:
                // Reduce commitment in target enemy room
                if (targetRoom != null)
                {
                    Vector3 targetPos = GetRoomWorldPosition(targetRoom);
                    var followers = targetRoom.Followers;
                    int damage = effectValue;
                    MaskProjectile.Create(srcPos, targetPos, type, () => {
                        foreach (var follower in followers)
                        {
                            follower.DecayCommitment(damage);
                        }
                        Debug.Log($"Whispers applied! {followers.Count} followers lost {damage} commitment.");
                    });
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
    
    // === Helper Methods for Projectiles ===
    
    /// <summary>
    /// Get the default source position for projectiles based on the cult's god.
    /// </summary>
    private Vector3 GetDefaultSourcePosition(Cult sourceCult)
    {
        // Try to get the god's visual position
        if (sourceCult?.god != null)
        {
            // Look for a GodVisual associated with this god
            var godVisuals = UnityEngine.Object.FindObjectsOfType<GodVisual>();
            foreach (var visual in godVisuals)
            {
                if (visual.God == sourceCult.god)
                {
                    return visual.transform.position;
                }
            }
        }
        
        // Fallback to a default position
        return new Vector3(-10f, 5f, 0f);
    }
    
    /// <summary>
    /// Get world position for a room target.
    /// </summary>
    private Vector3 GetRoomWorldPosition(Room room)
    {
        if (room == null) return Vector3.zero;
        
        // Try to find the room's visual
        var roomVisuals = UnityEngine.Object.FindObjectsOfType<RoomVisual>();
        foreach (var visual in roomVisuals)
        {
            if (visual.Room == room)
            {
                return visual.transform.position;
            }
        }
        
        // Fallback based on grid position
        return new Vector3(room.Location.x * 2f, room.Location.y * 2f, 0f);
    }
    
    /// <summary>
    /// Spawn a projectile with a delay (for multi-target effects).
    /// </summary>
    private void SpawnDelayedProjectile(Vector3 srcPos, Vector3 targetPos, MaskType maskType, Room targetRoom, int damage, float delay)
    {
        // Use a temporary MonoBehaviour to start a coroutine
        var helper = new GameObject("ProjectileSpawner");
        var spawner = helper.AddComponent<DelayedProjectileSpawner>();
        spawner.Spawn(srcPos, targetPos, maskType, targetRoom, damage, delay);
    }
    
    /// <summary>
    /// Check if this mask is a Shield type that can block attacks.
    /// </summary>
    public bool IsShield => type == MaskType.Shield;
}

/// <summary>
/// Helper MonoBehaviour for spawning delayed projectiles.
/// </summary>
public class DelayedProjectileSpawner : MonoBehaviour
{
    public void Spawn(Vector3 srcPos, Vector3 targetPos, MaskType maskType, Room targetRoom, int damage, float delay)
    {
        StartCoroutine(SpawnAfterDelay(srcPos, targetPos, maskType, targetRoom, damage, delay));
    }
    
    private System.Collections.IEnumerator SpawnAfterDelay(Vector3 srcPos, Vector3 targetPos, MaskType maskType, Room targetRoom, int damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        MaskProjectile.Create(srcPos, targetPos, maskType, () => {
            targetRoom?.TakeDamage(damage);
        });
        
        Destroy(gameObject);
    }
}
