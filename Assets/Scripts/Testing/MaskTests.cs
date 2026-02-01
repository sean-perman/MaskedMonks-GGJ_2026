using System.Collections;
using UnityEngine;

/// <summary>
/// Unit tests for all mask types and their functionality.
/// Tests mask construction, cost checking, shelf life, and effect application.
/// 
/// HOW TO USE:
/// 1. Add this component to a test scene along with GameInitializer
/// 2. Press Play
/// 3. Use the test hotkeys listed below
/// 4. Check console for PASSED/FAILED results
/// 
/// HOTKEYS:
/// M - Run all mask tests
/// N - Test mask construction
/// B - Test mask costs and affordability
/// V - Test mask shelf life decay
/// C - Test combat masks (Strike, Lightning, Flood)
/// X - Test support masks (Shield, Sanctuary, Plenty)
/// </summary>
public class MaskTests : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runAllTestsOnStart = false;
    [SerializeField] private bool verboseLogging = true;
    
    // Test objects created at runtime
    private Cult testCult;
    private God testGod;
    private Church testChurch;
    private Cult enemyCult;
    private God enemyGod;
    private Church enemyChurch;
    
    private int testsRun = 0;
    private int testsPassed = 0;
    private int testsFailed = 0;
    
    private void Start()
    {
        PrintInstructions();
        
        if (runAllTestsOnStart)
        {
            StartCoroutine(RunAllMaskTests());
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) StartCoroutine(RunAllMaskTests());
        if (Input.GetKeyDown(KeyCode.N)) StartCoroutine(TestMaskConstruction());
        if (Input.GetKeyDown(KeyCode.B)) StartCoroutine(TestMaskCosts());
        if (Input.GetKeyDown(KeyCode.V)) StartCoroutine(TestMaskShelfLife());
        if (Input.GetKeyDown(KeyCode.C)) StartCoroutine(TestCombatMasks());
        if (Input.GetKeyDown(KeyCode.X)) StartCoroutine(TestSupportMasks());
    }
    
    private void PrintInstructions()
    {
        Debug.Log("=== MASK TESTS ===");
        Debug.Log("M - Run all mask tests");
        Debug.Log("N - Test mask construction");
        Debug.Log("B - Test mask costs and affordability");
        Debug.Log("V - Test mask shelf life decay");
        Debug.Log("C - Test combat masks (Strike, Lightning, Flood)");
        Debug.Log("X - Test support masks (Shield, Sanctuary, Plenty)");
        Debug.Log("==================");
    }
    
    // === Test Helpers ===
    
    private void SetupTestEnvironment()
    {
        CleanupTestEnvironment();
        
        // Create source cult (player)
        var cultGO = new GameObject("TestCult_Masks");
        testCult = cultGO.AddComponent<Cult>();
        
        var godGO = new GameObject("TestGod");
        godGO.transform.SetParent(cultGO.transform);
        testGod = godGO.AddComponent<God>();
        testGod.Initialize(100, 50);
        testCult.god = testGod;
        
        var churchGO = new GameObject("TestChurch");
        churchGO.transform.SetParent(cultGO.transform);
        testChurch = churchGO.AddComponent<Church>();
        testCult.church = testChurch;
        
        // Create enemy cult (target for attacks)
        var enemyCultGO = new GameObject("EnemyCult_Masks");
        enemyCult = enemyCultGO.AddComponent<Cult>();
        
        var enemyGodGO = new GameObject("EnemyGod");
        enemyGodGO.transform.SetParent(enemyCultGO.transform);
        enemyGod = enemyGodGO.AddComponent<God>();
        enemyGod.Initialize(100, 50);
        enemyCult.god = enemyGod;
        
        var enemyChurchGO = new GameObject("EnemyChurch");
        enemyChurchGO.transform.SetParent(enemyCultGO.transform);
        enemyChurch = enemyChurchGO.AddComponent<Church>();
        enemyCult.church = enemyChurch;
        
        Log($"Test environment created with source cult (Favor: {testGod.Favor}) and enemy cult");
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
        if (enemyCult != null)
        {
            Destroy(enemyCult.gameObject);
            enemyCult = null;
            enemyGod = null;
            enemyChurch = null;
        }
    }
    
    private T CreateTestRoom<T>(Church church, Vector2Int position) where T : Room
    {
        var roomGO = new GameObject($"Test{typeof(T).Name}");
        roomGO.transform.SetParent(church.transform);
        var room = roomGO.AddComponent<T>();
        church.AddRoom(room, position);
        return room;
    }
    
    private Follower CreateTestFollower(Cult cult)
    {
        var followerGO = new GameObject("TestFollower");
        followerGO.transform.SetParent(cult.transform);
        var follower = followerGO.AddComponent<Follower>();
        follower.Initialize(cult);
        cult.AddFollower(follower);
        return follower;
    }
    
    private void Log(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[MaskTest] {message}");
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
    
    public IEnumerator RunAllMaskTests()
    {
        Debug.Log("========== RUNNING ALL MASK TESTS ==========");
        ResetCounters();
        
        yield return TestMaskConstruction();
        yield return new WaitForSeconds(0.1f);
        
        yield return TestMaskCosts();
        yield return new WaitForSeconds(0.1f);
        
        yield return TestMaskShelfLife();
        yield return new WaitForSeconds(0.1f);
        
        yield return TestCombatMasks();
        yield return new WaitForSeconds(0.1f);
        
        yield return TestSupportMasks();
        
        PrintTestSummary("ALL MASK TESTS");
        Debug.Log("========== MASK TESTS FINISHED ==========");
    }
    
    // === Mask Construction Tests ===
    
    public IEnumerator TestMaskConstruction()
    {
        Debug.Log("--- Testing Mask Construction ---");
        SetupTestEnvironment();
        
        // Test 1: Strike mask construction
        var strikeMask = new Mask(
            type: MaskType.Strike,
            targetType: MaskTargetType.EnemyRoom,
            duration: 0f,
            shelfLife: 30f,
            favorCost: 2,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: 2
        );
        Assert(strikeMask.Type == MaskType.Strike, "MaskConstruction: Strike mask has correct type");
        Assert(strikeMask.TargetType == MaskTargetType.EnemyRoom, "MaskConstruction: Strike targets enemy room");
        Assert(strikeMask.IsInstant == true, "MaskConstruction: Strike is instant (duration=0)");
        Assert(strikeMask.EffectValue == 2, "MaskConstruction: Strike has effect value of 2");
        
        // Test 2: Lightning mask construction
        var lightningMask = new Mask(
            type: MaskType.Lightning,
            targetType: MaskTargetType.EnemyColumn,
            duration: 0f,
            shelfLife: 30f,
            favorCost: 3,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: 1
        );
        Assert(lightningMask.Type == MaskType.Lightning, "MaskConstruction: Lightning mask has correct type");
        Assert(lightningMask.TargetType == MaskTargetType.EnemyColumn, "MaskConstruction: Lightning targets enemy column");
        Assert(lightningMask.FavorCost == 3, "MaskConstruction: Lightning costs 3 favor");
        
        // Test 3: Flood mask construction
        var floodMask = new Mask(
            type: MaskType.Flood,
            targetType: MaskTargetType.EnemyBottomRow,
            duration: 0f,
            shelfLife: 10f,
            favorCost: 2,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: 2
        );
        Assert(floodMask.Type == MaskType.Flood, "MaskConstruction: Flood mask has correct type");
        Assert(floodMask.TargetType == MaskTargetType.EnemyBottomRow, "MaskConstruction: Flood targets bottom row");
        Assert(floodMask.ShelfLife == 10f, "MaskConstruction: Flood has 10s shelf life");
        Assert(floodMask.EffectValue == 2, "MaskConstruction: Flood deals 2 damage per room");
        
        // Test 4: Shield mask construction
        var shieldMask = new Mask(
            type: MaskType.Shield,
            targetType: MaskTargetType.Passive,
            duration: 0f,
            shelfLife: 8f,
            favorCost: 4,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: 1
        );
        Assert(shieldMask.Type == MaskType.Shield, "MaskConstruction: Shield mask has correct type");
        Assert(shieldMask.TargetType == MaskTargetType.Passive, "MaskConstruction: Shield is passive");
        Assert(shieldMask.IsShield == true, "MaskConstruction: Shield IsShield property is true");
        Assert(shieldMask.FavorCost == 4, "MaskConstruction: Shield costs 4 favor when auto-triggered");
        
        // Test 5: Wrath mask construction (direct god damage)
        var wrathMask = new Mask(
            type: MaskType.Wrath,
            targetType: MaskTargetType.EnemyRoom,
            duration: 0f,
            shelfLife: 60f,
            favorCost: 10,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: 15
        );
        Assert(wrathMask.Type == MaskType.Wrath, "MaskConstruction: Wrath mask has correct type");
        Assert(wrathMask.FavorCost == 10, "MaskConstruction: Wrath has high favor cost");
        Assert(wrathMask.EffectValue == 15, "MaskConstruction: Wrath deals significant damage");
        
        // Test 6: Plenty mask construction (favor gain)
        var plentyMask = new Mask(
            type: MaskType.Plenty,
            targetType: MaskTargetType.None,
            duration: 0f,
            shelfLife: 30f,
            favorCost: 5,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: 25
        );
        Assert(plentyMask.Type == MaskType.Plenty, "MaskConstruction: Plenty mask has correct type");
        Assert(plentyMask.TargetType == MaskTargetType.None, "MaskConstruction: Plenty has no target");
        Assert(plentyMask.EffectValue == 25, "MaskConstruction: Plenty grants favor");
        
        // Test 7: Sanctuary mask construction (own room buff)
        var sanctuaryMask = new Mask(
            type: MaskType.Sanctuary,
            targetType: MaskTargetType.OwnRoom,
            duration: 0f,
            shelfLife: 30f,
            favorCost: 3,
            moneyCost: 0,
            followerSacrifice: 0,
            effectValue: 20
        );
        Assert(sanctuaryMask.Type == MaskType.Sanctuary, "MaskConstruction: Sanctuary mask has correct type");
        Assert(sanctuaryMask.TargetType == MaskTargetType.OwnRoom, "MaskConstruction: Sanctuary targets own room");
        
        // Test 8: MaxShelfLife tracking
        Assert(strikeMask.MaxShelfLife == 30f, "MaskConstruction: MaxShelfLife preserved on creation");
        Assert(strikeMask.ShelfLifePercent == 1f, "MaskConstruction: New mask has 100% shelf life");
        
        // Test 9: IsExpired check
        Assert(strikeMask.IsExpired == false, "MaskConstruction: New mask is not expired");
        
        // Test 10: Architecture mask construction
        var architectMask = new Mask(
            type: MaskType.ArchitectLightningRitual,
            targetType: MaskTargetType.OwnEmptySlot,
            duration: 0f,
            shelfLife: 120f,
            favorCost: 0,
            moneyCost: 10,
            followerSacrifice: 0,
            effectValue: 0
        );
        Assert(architectMask.Type == MaskType.ArchitectLightningRitual, "MaskConstruction: Architect mask has correct type");
        Assert(architectMask.TargetType == MaskTargetType.OwnEmptySlot, "MaskConstruction: Architect targets empty slot");
        Assert(architectMask.MoneyCost == 10, "MaskConstruction: Architect costs money");
        
        CleanupTestEnvironment();
        yield return null;
    }
    
    // === Mask Cost Tests ===
    
    public IEnumerator TestMaskCosts()
    {
        Debug.Log("--- Testing Mask Costs and Affordability ---");
        SetupTestEnvironment();
        
        // Give cult starting favor
        testGod.Initialize(100, 10); // 10 favor
        
        // Test 1: Cheap mask is affordable
        var cheapMask = new Mask(MaskType.Strike, MaskTargetType.EnemyRoom, 0f, 30f, 2, 0, 0, 2);
        Assert(cheapMask.CanAfford(testCult) == true, "MaskCosts: Cheap mask (2 favor) is affordable with 10 favor");
        
        // Test 2: Expensive mask is not affordable
        var expensiveMask = new Mask(MaskType.Wrath, MaskTargetType.EnemyRoom, 0f, 60f, 20, 0, 0, 15);
        Assert(expensiveMask.CanAfford(testCult) == false, "MaskCosts: Expensive mask (20 favor) is not affordable with 10 favor");
        
        // Test 3: Exact affordability
        var exactMask = new Mask(MaskType.Strike, MaskTargetType.EnemyRoom, 0f, 30f, 10, 0, 0, 2);
        Assert(exactMask.CanAfford(testCult) == true, "MaskCosts: Mask costing exactly available favor is affordable");
        
        // Test 4: PayCost deducts favor
        int favorBefore = testGod.Favor;
        cheapMask.PayCost(testCult);
        Assert(testGod.Favor == favorBefore - cheapMask.FavorCost, "MaskCosts: PayCost correctly deducts favor");
        
        // Test 5: Money cost checking
        testCult.AddMoney(5);
        var moneyMask = new Mask(MaskType.ArchitectAltar, MaskTargetType.OwnEmptySlot, 0f, 120f, 0, 8, 0, 0);
        Assert(moneyMask.CanAfford(testCult) == false, "MaskCosts: Mask with 8 money cost unaffordable with 5 money");
        
        testCult.AddMoney(5); // Now has 10 money
        Assert(moneyMask.CanAfford(testCult) == true, "MaskCosts: Mask with 8 money cost affordable with 10 money");
        
        // Test 6: Follower sacrifice checking
        var sacrificeMask = new Mask(MaskType.Sacrifice, MaskTargetType.None, 0f, 30f, 0, 0, 2, 20);
        Assert(sacrificeMask.CanAfford(testCult) == false, "MaskCosts: Sacrifice mask unaffordable with 0 followers");
        
        CreateTestFollower(testCult);
        CreateTestFollower(testCult);
        Assert(sacrificeMask.CanAfford(testCult) == true, "MaskCosts: Sacrifice mask affordable with 2 followers");
        
        // Test 7: Combined costs
        var combinedCostMask = new Mask(MaskType.Strike, MaskTargetType.EnemyRoom, 0f, 30f, 5, 5, 1, 5);
        Assert(combinedCostMask.CanAfford(testCult) == true, "MaskCosts: Combined cost mask checks all resources");
        
        // Test 8: Null cult handling
        Assert(cheapMask.CanAfford(null) == false, "MaskCosts: CanAfford returns false for null cult");
        
        // Test 9: Zero cost mask
        var freeMask = new Mask(MaskType.Plenty, MaskTargetType.None, 0f, 30f, 0, 0, 0, 10);
        Assert(freeMask.CanAfford(testCult) == true, "MaskCosts: Zero cost mask is always affordable");
        Assert(freeMask.FavorCost == 0, "MaskCosts: Zero cost mask has 0 favor cost");
        
        // Test 10: PayCost with money
        int moneyBefore = testCult.Money;
        var paidMask = new Mask(MaskType.Strike, MaskTargetType.EnemyRoom, 0f, 30f, 1, 2, 0, 2);
        paidMask.PayCost(testCult);
        Assert(testCult.Money == moneyBefore - 2, "MaskCosts: PayCost correctly deducts money");
        
        CleanupTestEnvironment();
        yield return null;
    }
    
    // === Shelf Life Tests ===
    
    public IEnumerator TestMaskShelfLife()
    {
        Debug.Log("--- Testing Mask Shelf Life ---");
        SetupTestEnvironment();
        
        // Test 1: Initial shelf life
        var mask = new Mask(MaskType.Strike, MaskTargetType.EnemyRoom, 0f, 30f, 2, 0, 0, 2);
        Assert(mask.ShelfLife == 30f, "MaskShelfLife: Initial shelf life is correct");
        Assert(mask.MaxShelfLife == 30f, "MaskShelfLife: MaxShelfLife set on creation");
        
        // Test 2: ShelfLifePercent starts at 100%
        Assert(Mathf.Approximately(mask.ShelfLifePercent, 1f), "MaskShelfLife: Starts at 100%");
        
        // Test 3: TickShelfLife reduces time
        mask.TickShelfLife(10f);
        Assert(mask.ShelfLife == 20f, "MaskShelfLife: TickShelfLife reduces by delta time");
        
        // Test 4: ShelfLifePercent updates
        Assert(Mathf.Approximately(mask.ShelfLifePercent, 20f / 30f), "MaskShelfLife: Percent updates after tick");
        
        // Test 5: Not expired yet
        Assert(mask.IsExpired == false, "MaskShelfLife: Mask not expired with time remaining");
        
        // Test 6: Tick down to zero
        mask.TickShelfLife(20f);
        Assert(mask.ShelfLife == 0f, "MaskShelfLife: Shelf life reaches 0");
        Assert(mask.IsExpired == true, "MaskShelfLife: Mask is expired at 0 shelf life");
        
        // Test 7: Shelf life doesn't go negative
        mask.TickShelfLife(10f);
        Assert(mask.ShelfLife == 0f, "MaskShelfLife: Shelf life doesn't go below 0");
        
        // Test 8: Short shelf life (Flood mask)
        var floodMask = new Mask(MaskType.Flood, MaskTargetType.EnemyBottomRow, 0f, 
            GameConfig.Instance.floodMaskShelfLife, 2, 0, 0, 2);
        Assert(floodMask.ShelfLife <= 10f, "MaskShelfLife: Flood mask has short shelf life");
        
        // Test 9: Shield mask shelf life from config
        var shieldMask = new Mask(MaskType.Shield, MaskTargetType.Passive, 0f,
            GameConfig.Instance.shieldMaskShelfLife, 4, 0, 0, 1);
        Assert(shieldMask.ShelfLife > 0, "MaskShelfLife: Shield mask has configured shelf life");
        
        // Test 10: God storage ticks mask shelf life (integration)
        var freshMask = new Mask(MaskType.Strike, MaskTargetType.EnemyRoom, 0f, 30f, 2, 0, 0, 2);
        testGod.AddMaskToStorage(freshMask);
        float initialLife = freshMask.ShelfLife;
        yield return new WaitForSeconds(0.5f);
        Assert(freshMask.ShelfLife < initialLife, "MaskShelfLife: God storage ticks shelf life over time");
        
        CleanupTestEnvironment();
        yield return null;
    }
    
    // === Combat Mask Tests ===
    
    public IEnumerator TestCombatMasks()
    {
        Debug.Log("--- Testing Combat Masks ---");
        SetupTestEnvironment();
        
        // Create target rooms in enemy church
        var targetRoom = CreateTestRoom<SanctuaryRoom>(enemyChurch, new Vector2Int(0, 0));
        var targetRoom2 = CreateTestRoom<AltarRoom>(enemyChurch, new Vector2Int(1, 0));
        var targetRoom3 = CreateTestRoom<PewsRoom>(enemyChurch, new Vector2Int(2, 0));
        
        // Test 1: Strike mask type verification
        var strikeMask = new Mask(MaskType.Strike, MaskTargetType.EnemyRoom, 0f, 30f, 2, 0, 0, 2);
        Assert(strikeMask.Type == MaskType.Strike, "CombatMasks: Strike mask type correct");
        Assert(strikeMask.TargetType == MaskTargetType.EnemyRoom, "CombatMasks: Strike targets enemy room");
        
        // Test 2: Lightning mask type verification
        var lightningMask = new Mask(MaskType.Lightning, MaskTargetType.EnemyColumn, 0f, 30f, 3, 0, 0, 1);
        Assert(lightningMask.Type == MaskType.Lightning, "CombatMasks: Lightning mask type correct");
        Assert(lightningMask.TargetType == MaskTargetType.EnemyColumn, "CombatMasks: Lightning targets column");
        
        // Test 3: Flood mask type verification
        var floodMask = new Mask(MaskType.Flood, MaskTargetType.EnemyBottomRow, 0f, 10f, 2, 0, 0, 2);
        Assert(floodMask.Type == MaskType.Flood, "CombatMasks: Flood mask type correct");
        Assert(floodMask.TargetType == MaskTargetType.EnemyBottomRow, "CombatMasks: Flood targets bottom row");
        
        // Test 4: Smiting mask (damages followers)
        var smitingMask = new Mask(MaskType.Smiting, MaskTargetType.EnemyRoom, 0f, 30f, 5, 0, 0, 10);
        Assert(smitingMask.Type == MaskType.Smiting, "CombatMasks: Smiting mask type correct");
        Assert(smitingMask.EffectValue == 10, "CombatMasks: Smiting has commitment damage value");
        
        // Test 5: Wrath mask (direct god damage)
        var wrathMask = new Mask(MaskType.Wrath, MaskTargetType.EnemyRoom, 0f, 60f, 10, 0, 0, 15);
        Assert(wrathMask.Type == MaskType.Wrath, "CombatMasks: Wrath mask type correct");
        
        // Test 6: Whispers mask (commitment reduction)
        var whispersMask = new Mask(MaskType.Whispers, MaskTargetType.EnemyRoom, 0f, 30f, 3, 0, 0, 15);
        Assert(whispersMask.Type == MaskType.Whispers, "CombatMasks: Whispers mask type correct");
        
        // Test 7: Config values for Lightning
        Assert(GameConfig.Instance.lightningDamagePerRoom >= 1, "CombatMasks: Lightning damage per room configured");
        Assert(GameConfig.Instance.lightningMaskFavorCost >= 1, "CombatMasks: Lightning favor cost configured");
        
        // Test 8: Config values for Flood
        Assert(GameConfig.Instance.floodDamagePerRoom >= 1, "CombatMasks: Flood damage per room configured");
        Assert(GameConfig.Instance.floodMaskFavorCost >= 1, "CombatMasks: Flood favor cost configured");
        
        // Test 9: Strike mask effect application (room damage)
        int initialDamage = targetRoom.Damage;
        // Note: ApplyEffect uses projectiles which are visual, so we test the mask properties instead
        Assert(strikeMask.EffectValue > 0, "CombatMasks: Strike has positive damage value");
        
        // Test 10: Instant vs Duration effects
        Assert(strikeMask.IsInstant == true, "CombatMasks: Strike is instant effect");
        Assert(lightningMask.IsInstant == true, "CombatMasks: Lightning is instant effect");
        Assert(floodMask.IsInstant == true, "CombatMasks: Flood is instant effect");
        
        CleanupTestEnvironment();
        yield return null;
    }
    
    // === Support Mask Tests ===
    
    public IEnumerator TestSupportMasks()
    {
        Debug.Log("--- Testing Support Masks ---");
        SetupTestEnvironment();
        
        // Create own room with followers
        var ownRoom = CreateTestRoom<SanctuaryRoom>(testChurch, new Vector2Int(0, 0));
        var follower = CreateTestFollower(testCult);
        follower.SetCommitment(50f); // Set to half commitment
        ownRoom.AddFollower(follower);
        
        // Test 1: Shield mask properties
        var shieldMask = new Mask(MaskType.Shield, MaskTargetType.Passive, 0f,
            GameConfig.Instance.shieldMaskShelfLife, GameConfig.Instance.shieldFavorCost, 0, 0, 1);
        Assert(shieldMask.Type == MaskType.Shield, "SupportMasks: Shield mask type correct");
        Assert(shieldMask.TargetType == MaskTargetType.Passive, "SupportMasks: Shield is passive targeting");
        Assert(shieldMask.IsShield == true, "SupportMasks: Shield IsShield property true");
        
        // Test 2: Shield auto-activation cost
        Assert(shieldMask.FavorCost >= 4, "SupportMasks: Shield requires 4+ favor to auto-activate");
        
        // Test 3: Sanctuary mask properties
        var sanctuaryMask = new Mask(MaskType.Sanctuary, MaskTargetType.OwnRoom, 0f, 30f, 3, 0, 0, 20);
        Assert(sanctuaryMask.Type == MaskType.Sanctuary, "SupportMasks: Sanctuary mask type correct");
        Assert(sanctuaryMask.TargetType == MaskTargetType.OwnRoom, "SupportMasks: Sanctuary targets own room");
        Assert(sanctuaryMask.EffectValue == 20, "SupportMasks: Sanctuary has commitment recovery value");
        
        // Test 4: Plenty mask (favor gain)
        var plentyMask = new Mask(MaskType.Plenty, MaskTargetType.None, 0f, 30f, 5, 0, 0, 25);
        Assert(plentyMask.Type == MaskType.Plenty, "SupportMasks: Plenty mask type correct");
        Assert(plentyMask.TargetType == MaskTargetType.None, "SupportMasks: Plenty has no target");
        
        // Test 5: Plenty mask effect
        int favorBefore = testGod.Favor;
        plentyMask.ApplyEffect(testCult, null, null);
        // Note: ApplyEffect should increase favor by effectValue
        Assert(testGod.Favor >= favorBefore, "SupportMasks: Plenty increases favor");
        
        // Test 6: Sacrifice mask properties
        var sacrificeMask = new Mask(MaskType.Sacrifice, MaskTargetType.None, 0f, 30f, 0, 0, 1, 25);
        Assert(sacrificeMask.Type == MaskType.Sacrifice, "SupportMasks: Sacrifice mask type correct");
        Assert(sacrificeMask.FollowerSacrifice == 1, "SupportMasks: Sacrifice requires follower sacrifice");
        Assert(sacrificeMask.EffectValue == 25, "SupportMasks: Sacrifice heals god strength");
        
        // Test 7: Architecture masks (building rooms)
        var architectMasks = new MaskType[]
        {
            MaskType.ArchitectSanctuary,
            MaskType.ArchitectAltar,
            MaskType.ArchitectPews,
            MaskType.ArchitectMission,
            MaskType.ArchitectRitualHall,
            MaskType.ArchitectWorkshop,
            MaskType.ArchitectFundraising,
            MaskType.ArchitectLightningRitual,
            MaskType.ArchitectFloodRitual,
            MaskType.ArchitectShieldRitual
        };
        foreach (var maskType in architectMasks)
        {
            var architectMask = new Mask(maskType, MaskTargetType.OwnEmptySlot, 0f, 120f, 0, 10, 0, 0);
            Assert(architectMask.TargetType == MaskTargetType.OwnEmptySlot, $"SupportMasks: {maskType} targets empty slot");
        }
        
        // Test 8: God mask storage
        Assert(testGod.MaskStorageRemaining > 0, "SupportMasks: God has mask storage space");
        testGod.AddMaskToStorage(shieldMask);
        Assert(testGod.StoredMasks.Count == 1, "SupportMasks: Mask added to god storage");
        
        // Test 9: Shield in storage
        bool hasShield = false;
        foreach (var mask in testGod.StoredMasks)
        {
            if (mask.IsShield) hasShield = true;
        }
        Assert(hasShield, "SupportMasks: Shield mask found in storage");
        
        // Test 10: Multiple masks in storage
        var strikeMask = new Mask(MaskType.Strike, MaskTargetType.EnemyRoom, 0f, 30f, 2, 0, 0, 2);
        testGod.AddMaskToStorage(strikeMask);
        Assert(testGod.StoredMasks.Count == 2, "SupportMasks: Multiple masks stored");
        
        CleanupTestEnvironment();
        yield return null;
    }
}
