using UnityEngine;

/// <summary>
/// Comprehensive tester for all game systems.
/// Add this to an empty GameObject along with GameInitializer.
/// 
/// HOW TO SET UP A TEST SCENE:
/// 1. Create a new empty scene
/// 2. Create an empty GameObject called "Game"
/// 3. Add these components to it:
///    - GameManager
///    - GameInitializer
///    - GameTester (this script)
///    - DebugDashboard (optional, for visual feedback)
/// 4. Press Play
/// 5. Use the hotkeys below to test systems
/// 
/// The GameInitializer will automatically create:
/// - Two cults (Cult 1 on the left, Cult 2 on the right)
/// - Each cult has a God, Church with rooms, and 5 followers
/// - Player controllers with cursor support
/// </summary>
public class GameTester : MonoBehaviour
{
    [Header("References (Auto-found if not set)")]
    [SerializeField] private GameInitializer initializer;
    [SerializeField] private GameManager gameManager;
    
    [Header("Test Settings")]
    [SerializeField] private int testDamageAmount = 10;
    [SerializeField] private int testHealAmount = 10;
    [SerializeField] private float testCommitmentChange = 20f;
    
    private void Start()
    {
        // Auto-find references
        if (initializer == null)
            initializer = FindFirstObjectByType<GameInitializer>();
        if (gameManager == null)
            gameManager = GameManager.Instance;
            
        LogHelp();
    }
    
    private void LogHelp()
    {
        Debug.Log("=== GAME TESTER HOTKEYS ===");
        Debug.Log("--- PLAYER CONTROLS ---");
        Debug.Log("P1: WASD=Move Cursor, Z=Send to Sanctuary, X=Send from Sanctuary, Q=Upgrade, 1-4=Masks");
        Debug.Log("P2: Arrows=Move Cursor, ,=Send to Sanctuary, .=Send from Sanctuary, /=Upgrade, Num1-4=Masks");
        Debug.Log("");
        Debug.Log("--- TEST KEYS ---");
        Debug.Log("F1 = Damage Cult 1 God");
        Debug.Log("F2 = Heal Cult 1 God");
        Debug.Log("F3 = Damage Cult 2 God");
        Debug.Log("F4 = Heal Cult 2 God");
        Debug.Log("F5 = Give Cult 1 a test mask");
        Debug.Log("F6 = Give Cult 2 a test mask");
        Debug.Log("F7 = Spawn follower for Cult 1");
        Debug.Log("F8 = Spawn follower for Cult 2");
        Debug.Log("F9 = Damage random room (Cult 1)");
        Debug.Log("F10 = Trigger god combat manually");
        Debug.Log("");
        Debug.Log("--- UI KEYS ---");
        Debug.Log("F12 = Toggle Debug Dashboard");
        Debug.Log("` (backtick) = Toggle Debug Console");
        Debug.Log("Escape = Controls Menu");
        Debug.Log("==========================");
    }
    
    private void Update()
    {
        // God damage/heal tests
        if (Input.GetKeyDown(KeyCode.F1)) TestDamageGod(1);
        if (Input.GetKeyDown(KeyCode.F2)) TestHealGod(1);
        if (Input.GetKeyDown(KeyCode.F3)) TestDamageGod(2);
        if (Input.GetKeyDown(KeyCode.F4)) TestHealGod(2);
        
        // Mask tests
        if (Input.GetKeyDown(KeyCode.F5)) TestGiveMask(1);
        if (Input.GetKeyDown(KeyCode.F6)) TestGiveMask(2);
        
        // Follower tests
        if (Input.GetKeyDown(KeyCode.F7)) TestSpawnFollower(1);
        if (Input.GetKeyDown(KeyCode.F8)) TestSpawnFollower(2);
        
        // Room tests
        if (Input.GetKeyDown(KeyCode.F9)) TestDamageRoom(1);
        
        // Combat tests
        if (Input.GetKeyDown(KeyCode.F10)) TestGodCombat();
        
        // Show help
        if (Input.GetKeyDown(KeyCode.H)) LogHelp();
    }
    
    private Cult GetCult(int cultNumber)
    {
        if (gameManager == null) return null;
        return cultNumber == 1 ? gameManager.Cult1 : gameManager.Cult2;
    }
    
    private void TestDamageGod(int cultNumber)
    {
        var cult = GetCult(cultNumber);
        if (cult?.god == null)
        {
            Debug.LogWarning($"Cult {cultNumber} or its god not found!");
            return;
        }
        
        cult.god.DecreaseStrength(testDamageAmount);
        Debug.Log($"[TEST] Damaged Cult {cultNumber} God for {testDamageAmount}. Strength: {cult.god.Strength}/{cult.god.MaxStrength}");
    }
    
    private void TestHealGod(int cultNumber)
    {
        var cult = GetCult(cultNumber);
        if (cult?.god == null)
        {
            Debug.LogWarning($"Cult {cultNumber} or its god not found!");
            return;
        }
        
        cult.god.IncreaseStrength(testHealAmount);
        Debug.Log($"[TEST] Healed Cult {cultNumber} God for {testHealAmount}. Strength: {cult.god.Strength}/{cult.god.MaxStrength}");
    }
    
    private void TestGiveMask(int cultNumber)
    {
        var cult = GetCult(cultNumber);
        if (cult?.god == null)
        {
            Debug.LogWarning($"Cult {cultNumber} or its god not found!");
            return;
        }
        
        // Create a random test mask
        MaskType[] types = { MaskType.Smiting, MaskType.Wrath, MaskType.Whispers, MaskType.Sanctuary, MaskType.Plenty };
        var randomType = types[Random.Range(0, types.Length)];
        
        var targetType = randomType == MaskType.Smiting || randomType == MaskType.Wrath || randomType == MaskType.Whispers 
            ? MaskTargetType.EnemyRoom 
            : MaskTargetType.OwnRoom;
        
        var mask = new Mask(
            type: randomType,
            targetType: targetType,
            duration: 0f, // instant
            shelfLife: 60f,
            favorCost: 10,
            effectValue: 15
        );
        
        bool added = cult.god.AddMaskToStorage(mask);
        if (added)
        {
            Debug.Log($"[TEST] Gave Cult {cultNumber} a {randomType} mask. Masks: {cult.god.Masks.Count}/4");
        }
        else
        {
            Debug.LogWarning($"[TEST] Cult {cultNumber} mask storage is full!");
        }
    }
    
    private void TestSpawnFollower(int cultNumber)
    {
        var cult = GetCult(cultNumber);
        if (cult == null)
        {
            Debug.LogWarning($"Cult {cultNumber} not found!");
            return;
        }
        
        var followerObj = new GameObject($"Test Follower {cult.FollowerCount + 1}");
        followerObj.transform.SetParent(cult.transform);
        
        var follower = followerObj.AddComponent<Follower>();
        follower.Initialize(cult);
        cult.AddFollower(follower);
        
        Debug.Log($"[TEST] Spawned follower for Cult {cultNumber}. Total: {cult.FollowerCount}");
    }
    
    private void TestDamageRoom(int cultNumber)
    {
        var cult = GetCult(cultNumber);
        if (cult?.church == null)
        {
            Debug.LogWarning($"Cult {cultNumber} or its church not found!");
            return;
        }
        
        var rooms = cult.church.Rooms;
        if (rooms.Count == 0)
        {
            Debug.LogWarning($"Cult {cultNumber} has no rooms!");
            return;
        }
        
        var randomRoom = rooms[Random.Range(0, rooms.Count)];
        randomRoom.TakeDamage(1);
        
        Debug.Log($"[TEST] Damaged {randomRoom.Type} in Cult {cultNumber}. Damage: {randomRoom.Damage}, Capacity: {randomRoom.Capacity}");
    }
    
    private void TestGodCombat()
    {
        var cult1 = GetCult(1);
        var cult2 = GetCult(2);
        
        if (cult1?.god == null || cult2?.god == null)
        {
            Debug.LogWarning("Both cults must have gods for combat!");
            return;
        }
        
        int damage1 = Mathf.Max(1, cult1.god.Strength / 10);
        int damage2 = Mathf.Max(1, cult2.god.Strength / 10);
        
        cult2.god.DecreaseStrength(damage1);
        cult1.god.DecreaseStrength(damage2);
        
        Debug.Log($"[TEST] God Combat! Cult1 dealt {damage1}, Cult2 dealt {damage2}");
        Debug.Log($"  Cult1 God: {cult1.god.Strength}/{cult1.god.MaxStrength}");
        Debug.Log($"  Cult2 God: {cult2.god.Strength}/{cult2.god.MaxStrength}");
    }
    
    // === GUI Help Display ===
    
    private void OnGUI()
    {
        // Show minimal help in corner
        GUI.color = new Color(1, 1, 1, 0.7f);
        GUI.Label(new Rect(10, 10, 300, 20), "Press H for hotkey help | F12 for Dashboard | ESC for Controls");
        GUI.color = Color.white;
    }
}
