using UnityEngine;

/// <summary>
/// Centralized configuration for all game parameters.
/// Create an instance via Assets > Create > MaskedMonks > Game Config
/// Then place it in a Resources folder.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "MaskedMonks/Game Config")]
public class GameConfig : ScriptableObject
{
    // ============================================
    // SINGLETON ACCESS
    // ============================================
    
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");
                if (_instance == null)
                {
                    Debug.LogWarning("GameConfig not found in Resources folder! Using defaults.");
                    _instance = CreateInstance<GameConfig>();
                }
            }
            return _instance;
        }
    }
    
    // ============================================
    // ROOM CONFIGURATIONS
    // ============================================
    
    [Header("=== ROOM SETTINGS ===")]
    
    [Header("Sanctuary")]
    [Tooltip("Commitment recovered per second while in Sanctuary")]
    public float sanctuaryCommitmentRecoveryPerSecond = 2f;
    
    [Header("Altar")]
    [Tooltip("Pawn-seconds to generate 1 strength")]
    public float altarDuration = 30f;
    [Tooltip("Strength generated per trigger")]
    public int altarStrengthPerTrigger = 3;
    
    [Header("Pews")]
    [Tooltip("Pawn-seconds to generate 1 favor")]
    public float pewsDuration = 10f;
    [Tooltip("Favor generated per trigger")]
    public int pewsFavorPerTrigger = 1;
    
    [Header("Mission")]
    [Tooltip("Pawn-seconds to recruit 1 follower")]
    public float missionDuration = 40f;
    [Tooltip("Followers recruited per trigger")]
    public int missionFollowersPerTrigger = 1;
    
    [Header("Fundraising")]
    [Tooltip("Pawn-seconds to generate money")]
    public float fundraisingDuration = 30f;
    [Tooltip("Money generated per trigger")]
    public int fundraisingMoneyPerTrigger = 3;
    [Tooltip("Favor cost per trigger")]
    public int fundraisingFavorCost = 2;
    
    [Header("Workshop (Room Repairs)")]
    [Tooltip("Pawn-seconds to repair all damaged rooms (DEPRECATED - Workshop uses constant 20s)")]
    [System.Obsolete("Workshop now uses internal constant. This field is kept for compatibility.")]
    public float workshopBlueprintDuration = 20f;
    [Tooltip("DEPRECATED - Architecture masks removed")]
    [System.Obsolete("Architecture masks have been removed.")]
    public float workshopBlueprintShelfLife = 120f;
    
    [Header("Ritual Hall (Strike Mask)")]
    [Tooltip("Pawn-seconds to generate a Strike mask")]
    public float ritualHallDuration = 15f;
    [Tooltip("Favor cost for Strike masks")]
    public int ritualHallMaskFavorCost = 2;
    [Tooltip("Damage dealt by Strike mask")]
    public int ritualHallMaskEffectValue = 1;
    [Tooltip("Shelf life for Strike masks (seconds)")]
    public float ritualHallMaskShelfLife = 30f;
    
    [Header("Lightning Ritual")]
    [Tooltip("Pawn-seconds to generate a Lightning mask")]
    public float lightningRitualDuration = 25f;
    [Tooltip("Favor cost for Lightning masks")]
    public int lightningMaskFavorCost = 3;
    [Tooltip("Damage per room in column")]
    public int lightningDamagePerRoom = 1;
    [Tooltip("Shelf life for Lightning masks (seconds)")]
    public float lightningMaskShelfLife = 30f;
    
    [Header("Flood Ritual")]
    [Tooltip("Pawn-seconds to generate a Flood mask")]
    public float floodRitualDuration = 60f;
    [Tooltip("Favor cost for Flood masks")]
    public int floodMaskFavorCost = 2;
    [Tooltip("Damage per room in bottom row")]
    public int floodDamagePerRoom = 2;
    [Tooltip("Shelf life for Flood masks (seconds)")]
    public float floodMaskShelfLife = 10f;
    
    [Header("Shield Ritual")]
    [Tooltip("Pawn-seconds to generate a Shield mask")]
    public float shieldRitualDuration = 35f;
    [Tooltip("Favor cost when Shield auto-activates")]
    public int shieldFavorCost = 4;
    [Tooltip("Shelf life for Shield masks (seconds)")]
    public float shieldMaskShelfLife = 8f;
    
    // ============================================
    // ROOM BUILD COSTS (Gold)
    // ============================================
    
    [Header("=== ROOM BUILD COSTS (Gold) ===")]
    public int sanctuaryBuildCost = 5;
    public int altarBuildCost = 8;
    public int pewsBuildCost = 6;
    public int missionBuildCost = 10;
    public int fundraisingBuildCost = 7;
    public int workshopBuildCost = 10;
    public int ritualHallBuildCost = 10;
    public int lightningRitualBuildCost = 10;
    public int floodRitualBuildCost = 10;
    public int shieldRitualBuildCost = 10;
    
    // ============================================
    // ROOM MECHANICS
    // ============================================
    
    [Header("=== ROOM MECHANICS ===")]
    [Tooltip("Commitment damage when a room level is damaged")]
    public float commitmentDamagePerLevel = 15f;
    [Tooltip("Repair progress per follower per second")]
    public float repairRatePerFollowerPerSecond = 0.1f;
    [Tooltip("Accumulated repair needed to fix one damage level")]
    public float repairThreshold = 1f;
    [Tooltip("Maximum room level")]
    public int maxRoomLevel = 5;
    
    // ============================================
    // FOLLOWER SETTINGS
    // ============================================
    
    [Header("=== FOLLOWER SETTINGS ===")]
    [Tooltip("Starting commitment for new followers")]
    public float followerStartingCommitment = 100f;
    [Tooltip("Maximum commitment")]
    public float followerMaxCommitment = 100f;
    [Tooltip("Base commitment decay rate per second (in working rooms)")]
    public float followerDecayRatePerSecond = 1f;
    
    // ============================================
    // GOD SETTINGS
    // ============================================
    
    [Header("=== GOD SETTINGS ===")]
    [Tooltip("Maximum god strength")]
    public int godMaxStrength = 100;
    [Tooltip("Starting god strength")]
    public int godStartingStrength = 50;
    [Tooltip("Starting favor")]
    public int godStartingFavor = 5;
    [Tooltip("Starting money")]
    public int godStartingMoney = 10;
    [Tooltip("Maximum mask storage slots")]
    public int godMaxStoredMasks = 5;
    [Tooltip("Passive strength regeneration per second")]
    public float godPassiveRegenRate = 0f;
    
    // ============================================
    // HELPER METHODS
    // ============================================
    
    /// <summary>
    /// Get the build cost for a room type.
    /// </summary>
    public int GetRoomBuildCost(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Sanctuary => sanctuaryBuildCost,
            RoomType.Altar => altarBuildCost,
            RoomType.Pews => pewsBuildCost,
            RoomType.Mission => missionBuildCost,
            RoomType.Fundraising => fundraisingBuildCost,
            RoomType.Workshop => workshopBuildCost,
            RoomType.WrathRitualHall => ritualHallBuildCost,
            RoomType.LightningRitual => lightningRitualBuildCost,
            RoomType.FloodRitual => floodRitualBuildCost,
            RoomType.ShieldRitual => shieldRitualBuildCost,
            _ => 10
        };
    }
    
    /// <summary>
    /// Get the duration for a room type.
    /// </summary>
    public float GetRoomDuration(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Altar => altarDuration,
            RoomType.Pews => pewsDuration,
            RoomType.Mission => missionDuration,
            RoomType.Fundraising => fundraisingDuration,
            RoomType.Workshop => workshopBlueprintDuration,
            RoomType.WrathRitualHall => ritualHallDuration,
            RoomType.LightningRitual => lightningRitualDuration,
            RoomType.FloodRitual => floodRitualDuration,
            RoomType.ShieldRitual => shieldRitualDuration,
            _ => 30f
        };
    }
}
