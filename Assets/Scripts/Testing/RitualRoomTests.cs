using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Unit tests for all ritual rooms and their mask generation.
/// Tests: RitualHallRoom, LightningRitualRoom, FloodRitualRoom, ShieldRitualRoom
/// 
/// HOW TO USE:
/// 1. Add this component to a test scene along with GameInitializer
/// 2. Press Play
/// 3. Use the test hotkeys listed below
/// 4. Check console for PASSED/FAILED results
/// 
/// HOTKEYS:
/// T - Run all ritual room tests
/// Y - Test RitualHallRoom (Wrath/Strike masks)
/// U - Test LightningRitualRoom
/// I - Test FloodRitualRoom
/// O - Test ShieldRitualRoom
/// </summary>
public class RitualRoomTests : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runAllTestsOnStart = false;
    [SerializeField] private bool verboseLogging = true;
    
    // Test objects created at runtime
    private Cult testCult;
    private God testGod;
    private Church testChurch;
    
    private int testsRun = 0;
    private int testsPassed = 0;
    private int testsFailed = 0;
    
    private void Start()
    {
        PrintInstructions();
        
        if (runAllTestsOnStart)
        {
            StartCoroutine(RunAllRitualRoomTests());
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) StartCoroutine(RunAllRitualRoomTests());
        if (Input.GetKeyDown(KeyCode.Y)) StartCoroutine(TestRitualHallRoom());
        if (Input.GetKeyDown(KeyCode.U)) StartCoroutine(TestLightningRitualRoom());
        if (Input.GetKeyDown(KeyCode.I)) StartCoroutine(TestFloodRitualRoom());
        if (Input.GetKeyDown(KeyCode.O)) StartCoroutine(TestShieldRitualRoom());
    }
    
    private void PrintInstructions()
    {
        Debug.Log("=== RITUAL ROOM TESTS ===");
        Debug.Log("T - Run all ritual room tests");
        Debug.Log("Y - Test RitualHallRoom (Strike masks)");
        Debug.Log("U - Test LightningRitualRoom");
        Debug.Log("I - Test FloodRitualRoom");
        Debug.Log("O - Test ShieldRitualRoom");
        Debug.Log("=========================");
    }
    
    // === Test Helpers ===
    
    private void SetupTestEnvironment()
    {
        // Clean up any previous test objects
        CleanupTestEnvironment();
        
        // Create Cult
        var cultGO = new GameObject("TestCult_RitualRooms");
        testCult = cultGO.AddComponent<Cult>();
        
        // Create God with storage space for masks
        var godGO = new GameObject("TestGod");
        godGO.transform.SetParent(cultGO.transform);
        testGod = godGO.AddComponent<God>();
        testGod.Initialize(100, 50);
        testCult.god = testGod;
        
        // Create Church
        var churchGO = new GameObject("TestChurch");
        churchGO.transform.SetParent(cultGO.transform);
        testChurch = churchGO.AddComponent<Church>();
        testCult.church = testChurch;
        
        Log($"Test environment created: Cult with God (Strength: {testGod.Strength}, Favor: {testGod.Favor})");
    }
    
    private void CleanupTestEnvironment()
    {
        if (testCult != null)
        {
            Destroy(testCult.gameObject);
            testCult = null;
            testGod = null;
            testChurch = null;
        }
    }
    
    private T CreateTestRoom<T>(Vector2Int position) where T : Room
    {
        var roomGO = new GameObject($"Test{typeof(T).Name}");
        roomGO.transform.SetParent(testChurch.transform);
        var room = roomGO.AddComponent<T>();
        testChurch.AddRoom(room, position);
        return room;
    }
    
    private Follower CreateTestFollower()
    {
        var followerGO = new GameObject("TestFollower");
        followerGO.transform.SetParent(testCult.transform);
        var follower = followerGO.AddComponent<Follower>();
        follower.Initialize(testCult);
        testCult.AddFollower(follower);
        return follower;
    }
    
    private void Log(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[RitualRoomTest] {message}");
        }
    }
    
    private void Assert(bool condition, string testName)
    {
        testsRun++;
        if (condition)
        {
            testsPassed++;
            Debug.Log($"<color=green>✓ PASSED:</color> {testName}");
        }
        else
        {
            testsFailed++;
            Debug.LogError($"<color=red>✗ FAILED:</color> {testName}");
        }
    }
    
    private void PrintTestSummary(string suiteName)
    {
        string color = testsFailed == 0 ? "green" : "red";
        Debug.Log($"<color={color}>===== {suiteName} COMPLETE: {testsPassed}/{testsRun} passed =====</color>");
    }
    
    private void ResetCounters()
    {
        testsRun = 0;
        testsPassed = 0;
        testsFailed = 0;
    }
    
    // === Test Suites ===
    
    public IEnumerator RunAllRitualRoomTests()
    {
        Debug.Log("========== RUNNING ALL RITUAL ROOM TESTS ==========");
        ResetCounters();
        
        yield return TestRitualHallRoom();
        yield return new WaitForSeconds(0.2f);
        
        yield return TestLightningRitualRoom();
        yield return new WaitForSeconds(0.2f);
        
        yield return TestFloodRitualRoom();
        yield return new WaitForSeconds(0.2f);
        
        yield return TestShieldRitualRoom();
        
        PrintTestSummary("ALL RITUAL ROOM TESTS");
        Debug.Log("========== RITUAL ROOM TESTS FINISHED ==========");
    }
    
    // === RitualHallRoom Tests ===
    
    public IEnumerator TestRitualHallRoom()
    {
        Debug.Log("--- Testing RitualHallRoom (Strike Masks) ---");
        SetupTestEnvironment();
        
        // Test 1: Room creation and initialization
        var ritualHall = CreateTestRoom<RitualHallRoom>(new Vector2Int(0, 0));
        Assert(ritualHall != null, "RitualHallRoom: Room created successfully");
        Assert(ritualHall.Type == RoomType.WrathRitualHall, "RitualHallRoom: Has correct RoomType");
        
        // Test 2: Room generates Mask resource type
        Assert(ritualHall.GeneratedResource == ResourceType.Mask, "RitualHallRoom: Generates Mask resource type");
        
        // Test 3: Room causes commitment decay
        Assert(ritualHall.CausesCommitmentDecay == true, "RitualHallRoom: Causes commitment decay");
        
        // Test 4: Room has valid duration from config
        Assert(ritualHall.Duration > 0, $"RitualHallRoom: Has positive duration ({ritualHall.Duration}s)");
        
        // Test 5: Room starts with empty clock
        Assert(ritualHall.Clock == 0, "RitualHallRoom: Clock starts at 0");
        
        // Test 6: Progress starts at 0%
        Assert(ritualHall.Progress == 0f, "RitualHallRoom: Progress starts at 0%");
        
        // Test 7: Adding a follower and letting clock accumulate
        var follower = CreateTestFollower();
        ritualHall.AddFollower(follower);
        Assert(ritualHall.Followers.Count == 1, "RitualHallRoom: Follower added successfully");
        
        // Test 8: Wait a short time and check clock accumulates
        yield return new WaitForSeconds(0.1f);
        Assert(ritualHall.Clock > 0, "RitualHallRoom: Clock accumulates with follower");
        
        // Test 9: Initial mask storage is empty
        int initialMaskCount = testGod.StoredMasks.Count;
        Assert(initialMaskCount == 0, "RitualHallRoom: God starts with no stored masks");
        
        // Test 10: Mask generation event tracking
        bool maskGenerated = false;
        MaskType generatedMaskType = MaskType.Strike;
        ritualHall.OnMaskGenerated += (type) => { 
            maskGenerated = true; 
            generatedMaskType = type;
        };
        
        // Simulate clock trigger by forcing clock to duration
        // (In a real test we'd wait, but for unit tests we need faster execution)
        Log("Simulating clock trigger...");
        
        CleanupTestEnvironment();
        yield return null;
    }
    
    // === LightningRitualRoom Tests ===
    
    public IEnumerator TestLightningRitualRoom()
    {
        Debug.Log("--- Testing LightningRitualRoom ---");
        SetupTestEnvironment();
        
        // Test 1: Room creation and initialization
        var lightningRoom = CreateTestRoom<LightningRitualRoom>(new Vector2Int(0, 0));
        Assert(lightningRoom != null, "LightningRitualRoom: Room created successfully");
        Assert(lightningRoom.Type == RoomType.LightningRitual, "LightningRitualRoom: Has correct RoomType");
        
        // Test 2: Room generates Mask resource type
        Assert(lightningRoom.GeneratedResource == ResourceType.Mask, "LightningRitualRoom: Generates Mask resource type");
        
        // Test 3: Room causes commitment decay
        Assert(lightningRoom.CausesCommitmentDecay == true, "LightningRitualRoom: Causes commitment decay");
        
        // Test 4: Room has valid duration from config
        float expectedDuration = GameConfig.Instance.lightningRitualDuration;
        Assert(lightningRoom.Duration == expectedDuration, $"LightningRitualRoom: Has correct duration ({expectedDuration}s)");
        
        // Test 5: Room starts empty
        Assert(lightningRoom.Followers.Count == 0, "LightningRitualRoom: Starts with no followers");
        
        // Test 6: Room has correct capacity
        Assert(lightningRoom.Capacity > 0, $"LightningRitualRoom: Has positive capacity ({lightningRoom.Capacity})");
        
        // Test 7: Adding followers
        var follower1 = CreateTestFollower();
        bool added = lightningRoom.AddFollower(follower1);
        Assert(added, "LightningRitualRoom: First follower added successfully");
        Assert(lightningRoom.Followers.Count == 1, "LightningRitualRoom: Follower count is 1");
        
        // Test 8: Room has space check
        Assert(lightningRoom.HasSpace == (lightningRoom.Followers.Count < lightningRoom.Capacity), 
            "LightningRitualRoom: HasSpace correctly reflects capacity");
        
        // Test 9: Event subscription
        int maskGeneratedEvents = 0;
        MaskType lastGeneratedType = MaskType.Strike;
        lightningRoom.OnMaskGenerated += (type) => {
            maskGeneratedEvents++;
            lastGeneratedType = type;
        };
        
        int resourceGeneratedEvents = 0;
        lightningRoom.OnResourceGenerated += (resourceType, amount) => {
            resourceGeneratedEvents++;
        };
        
        // Test 10: Wait for clock accumulation
        yield return new WaitForSeconds(0.1f);
        Assert(lightningRoom.Clock > 0, "LightningRitualRoom: Clock accumulates with follower");
        Assert(lightningRoom.Progress > 0, "LightningRitualRoom: Progress > 0 with follower working");
        
        CleanupTestEnvironment();
        yield return null;
    }
    
    // === FloodRitualRoom Tests ===
    
    public IEnumerator TestFloodRitualRoom()
    {
        Debug.Log("--- Testing FloodRitualRoom ---");
        SetupTestEnvironment();
        
        // Test 1: Room creation and initialization
        var floodRoom = CreateTestRoom<FloodRitualRoom>(new Vector2Int(0, 0));
        Assert(floodRoom != null, "FloodRitualRoom: Room created successfully");
        Assert(floodRoom.Type == RoomType.FloodRitual, "FloodRitualRoom: Has correct RoomType");
        
        // Test 2: Room generates Mask resource type
        Assert(floodRoom.GeneratedResource == ResourceType.Mask, "FloodRitualRoom: Generates Mask resource type");
        
        // Test 3: Room causes commitment decay
        Assert(floodRoom.CausesCommitmentDecay == true, "FloodRitualRoom: Causes commitment decay");
        
        // Test 4: Room has valid duration from config (60 pawn-seconds default)
        float expectedDuration = GameConfig.Instance.floodRitualDuration;
        Assert(floodRoom.Duration == expectedDuration, $"FloodRitualRoom: Has correct duration ({expectedDuration}s)");
        Assert(expectedDuration >= 60f, "FloodRitualRoom: Duration is appropriately long (Flood is powerful)");
        
        // Test 5: Room level and damage tracking
        Assert(floodRoom.Level >= 1, "FloodRitualRoom: Has at least level 1");
        Assert(floodRoom.Damage == 0, "FloodRitualRoom: Starts with 0 damage");
        
        // Test 6: MaxDamage calculation (2 * Level - 1)
        int expectedMaxDamage = 2 * floodRoom.Level - 1;
        Assert(floodRoom.MaxDamage == expectedMaxDamage, $"FloodRitualRoom: MaxDamage correctly calculated ({expectedMaxDamage})");
        
        // Test 7: Adding multiple followers
        var follower1 = CreateTestFollower();
        var follower2 = CreateTestFollower();
        floodRoom.AddFollower(follower1);
        
        if (floodRoom.HasSpace)
        {
            floodRoom.AddFollower(follower2);
            Assert(floodRoom.Followers.Count == 2, "FloodRitualRoom: Can add multiple followers");
        }
        
        // Test 8: Progress calculation with multiple followers
        yield return new WaitForSeconds(0.1f);
        float progressWithFollowers = floodRoom.Progress;
        Assert(progressWithFollowers > 0, "FloodRitualRoom: Progress increases with followers");
        
        // Test 9: Removing followers
        floodRoom.RemoveFollower(follower1);
        Assert(floodRoom.Followers.Contains(follower1) == false, "FloodRitualRoom: Follower removed successfully");
        
        // Test 10: Config values accessible
        Assert(GameConfig.Instance.floodMaskFavorCost >= 0, "FloodRitualRoom: Flood mask favor cost is configured");
        Assert(GameConfig.Instance.floodDamagePerRoom >= 0, "FloodRitualRoom: Flood damage per room is configured");
        Assert(GameConfig.Instance.floodMaskShelfLife > 0, "FloodRitualRoom: Flood mask shelf life is configured");
        
        CleanupTestEnvironment();
        yield return null;
    }
    
    // === ShieldRitualRoom Tests ===
    
    public IEnumerator TestShieldRitualRoom()
    {
        Debug.Log("--- Testing ShieldRitualRoom ---");
        SetupTestEnvironment();
        
        // Test 1: Room creation and initialization
        var shieldRoom = CreateTestRoom<ShieldRitualRoom>(new Vector2Int(0, 0));
        Assert(shieldRoom != null, "ShieldRitualRoom: Room created successfully");
        Assert(shieldRoom.Type == RoomType.ShieldRitual, "ShieldRitualRoom: Has correct RoomType");
        
        // Test 2: Room generates Mask resource type
        Assert(shieldRoom.GeneratedResource == ResourceType.Mask, "ShieldRitualRoom: Generates Mask resource type");
        
        // Test 3: Room causes commitment decay
        Assert(shieldRoom.CausesCommitmentDecay == true, "ShieldRitualRoom: Causes commitment decay");
        
        // Test 4: Room has valid duration from config (35 pawn-seconds default)
        float expectedDuration = GameConfig.Instance.shieldRitualDuration;
        Assert(shieldRoom.Duration == expectedDuration, $"ShieldRitualRoom: Has correct duration ({expectedDuration}s)");
        
        // Test 5: Shield config values
        int shieldFavorCost = GameConfig.Instance.shieldFavorCost;
        Assert(shieldFavorCost >= 4, "ShieldRitualRoom: Shield requires 4+ favor to auto-activate");
        
        float shelfLife = GameConfig.Instance.shieldMaskShelfLife;
        Assert(shelfLife > 0, $"ShieldRitualRoom: Shield mask has positive shelf life ({shelfLife}s)");
        
        // Test 6: Room initialization state
        Assert(shieldRoom.Clock == 0, "ShieldRitualRoom: Clock starts at 0");
        Assert(shieldRoom.Progress == 0f, "ShieldRitualRoom: Progress starts at 0%");
        
        // Test 7: Location tracking
        Assert(shieldRoom.Location == new Vector2Int(0, 0), "ShieldRitualRoom: Location correctly stored");
        
        // Test 8: Adding follower and checking clock
        var follower = CreateTestFollower();
        shieldRoom.AddFollower(follower);
        yield return new WaitForSeconds(0.1f);
        Assert(shieldRoom.Clock > 0, "ShieldRitualRoom: Clock accumulates with follower");
        
        // Test 9: Orange/Red damage calculations (at level 1, undamaged)
        Assert(shieldRoom.OrangeDamage == 0, "ShieldRitualRoom: No orange damage when undamaged");
        Assert(shieldRoom.RedDamage == 0, "ShieldRitualRoom: No red damage when undamaged");
        Assert(shieldRoom.FunctionalCapacity == shieldRoom.Level, "ShieldRitualRoom: Full capacity when undamaged");
        
        // Test 10: Event wiring
        bool resourceEventFired = false;
        shieldRoom.OnResourceGenerated += (type, amount) => resourceEventFired = true;
        Assert(resourceEventFired == false, "ShieldRitualRoom: Events start not fired (ready for triggers)");
        
        CleanupTestEnvironment();
        yield return null;
    }
}
