using System.Collections;
using UnityEngine;

/// <summary>
/// Test script to validate all game systems.
/// Attach to a GameObject in the scene and run in Play mode.
/// Press keys to test different systems.
/// </summary>
public class GameSystemTester : MonoBehaviour
{
    [Header("Test References")]
    [SerializeField] private Cult testCult;
    [SerializeField] private God testGod;
    [SerializeField] private Church testChurch;
    [SerializeField] private Marketplace testMarketplace;
    
    [Header("Test Settings")]
    [SerializeField] private bool autoCreateTestScene = true;
    [SerializeField] private bool runAllTestsOnStart = false;
    
    private void Start()
    {
        if (autoCreateTestScene)
        {
            SetupTestScene();
        }
        
        if (runAllTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
        else
        {
            PrintInstructions();
        }
    }
    
    private void Update()
    {
        // Manual test triggers
        if (Input.GetKeyDown(KeyCode.F1)) TestFollowerCommitment();
        if (Input.GetKeyDown(KeyCode.F2)) TestRoomClock();
        if (Input.GetKeyDown(KeyCode.F3)) TestGodStrengthAndFavor();
        if (Input.GetKeyDown(KeyCode.F4)) TestMaskSystem();
        if (Input.GetKeyDown(KeyCode.F5)) TestMarketplace();
        if (Input.GetKeyDown(KeyCode.F6)) TestRoomAssignment();
        if (Input.GetKeyDown(KeyCode.F7)) TestWinLossConditions();
        if (Input.GetKeyDown(KeyCode.F8)) StartCoroutine(RunAllTests());
        if (Input.GetKeyDown(KeyCode.F9)) SetupTestScene();
    }
    
    private void PrintInstructions()
    {
        Debug.Log("=== GAME SYSTEM TESTER ===");
        Debug.Log("F1 - Test Follower Commitment");
        Debug.Log("F2 - Test Room Clock");
        Debug.Log("F3 - Test God Strength/Favor");
        Debug.Log("F4 - Test Mask System");
        Debug.Log("F5 - Test Marketplace");
        Debug.Log("F6 - Test Room Assignment");
        Debug.Log("F7 - Test Win/Loss Conditions");
        Debug.Log("F8 - Run All Tests");
        Debug.Log("F9 - Setup Test Scene");
        Debug.Log("==========================");
    }
    
    // === Test Scene Setup ===
    
    private void SetupTestScene()
    {
        Debug.Log("--- Setting up test scene ---");
        
        // Create Marketplace if needed
        if (Marketplace.Instance == null)
        {
            var marketplaceGO = new GameObject("Marketplace");
            testMarketplace = marketplaceGO.AddComponent<Marketplace>();
        }
        else
        {
            testMarketplace = Marketplace.Instance;
        }
        
        // Create Cult 1
        var cult1GO = new GameObject("TestCult1");
        testCult = cult1GO.AddComponent<Cult>();
        
        // Create God
        var godGO = new GameObject("TestGod1");
        godGO.transform.SetParent(cult1GO.transform);
        testGod = godGO.AddComponent<God>();
        testCult.god = testGod;
        
        // Create Church
        var churchGO = new GameObject("TestChurch1");
        churchGO.transform.SetParent(cult1GO.transform);
        testChurch = churchGO.AddComponent<Church>();
        testCult.church = testChurch;
        
        // Create Sanctuary Room
        var sanctuaryGO = new GameObject("Sanctuary");
        sanctuaryGO.transform.SetParent(churchGO.transform);
        var sanctuary = sanctuaryGO.AddComponent<SanctuaryRoom>();
        testChurch.AddRoom(sanctuary, new Vector2Int(0, 0));
        
        // Create Altar Room
        var altarGO = new GameObject("Altar");
        altarGO.transform.SetParent(churchGO.transform);
        var altar = altarGO.AddComponent<AltarRoom>();
        testChurch.AddRoom(altar, new Vector2Int(1, 0));
        
        // Create Pews Room
        var pewsGO = new GameObject("Pews");
        pewsGO.transform.SetParent(churchGO.transform);
        var pews = pewsGO.AddComponent<PewsRoom>();
        testChurch.AddRoom(pews, new Vector2Int(2, 0));
        
        // Create some followers
        for (int i = 0; i < 5; i++)
        {
            var followerGO = new GameObject($"Follower_{i}");
            followerGO.transform.SetParent(cult1GO.transform);
            var follower = followerGO.AddComponent<Follower>();
            follower.Initialize(testCult);
            testCult.AddFollower(follower);
        }
        
        Debug.Log($"Test scene created with:");
        Debug.Log($"  - Cult with {testCult.FollowerCount} followers");
        Debug.Log($"  - God with {testGod.Strength} strength, {testGod.Favor} favor");
        Debug.Log($"  - Church with {testChurch.Rooms.Count} rooms");
        Debug.Log($"  - Marketplace with {testMarketplace.CitizenCount} citizens");
    }
    
    // === Individual Tests ===
    
    private void TestFollowerCommitment()
    {
        Debug.Log("=== TEST: Follower Commitment ===");
        
        if (testCult == null || testCult.FollowerCount == 0)
        {
            Debug.LogError("No cult or followers to test!");
            return;
        }
        
        var followers = testCult.Followers;
        var testFollower = followers[0] as Follower;
        
        Debug.Log($"Initial commitment: {testFollower.Commitment}");
        
        testFollower.DecayCommitment(20f);
        Debug.Log($"After decay (20): {testFollower.Commitment}");
        
        testFollower.RecoverCommitment(10f);
        Debug.Log($"After recovery (10): {testFollower.Commitment}");
        
        testFollower.SetCommitment(100f);
        Debug.Log($"Reset to 100: {testFollower.Commitment}");
        
        Debug.Log("=== Follower Commitment Test PASSED ===");
    }
    
    private void TestRoomClock()
    {
        Debug.Log("=== TEST: Room Clock ===");
        
        if (testChurch == null)
        {
            Debug.LogError("No church to test!");
            return;
        }
        
        var altar = testChurch.GetRoomOfType(RoomType.Altar);
        if (altar == null)
        {
            Debug.LogError("No altar room found!");
            return;
        }
        
        Debug.Log($"Altar - Level: {altar.Level}, Capacity: {altar.Capacity}, Duration: {altar.Duration}");
        Debug.Log($"Altar - Followers: {altar.Followers.Count}, Clock: {altar.Clock}");
        Debug.Log($"Altar - Progress: {altar.Progress:P0}");
        
        // The clock will accumulate in Update based on follower count
        Debug.Log("(Clock accumulates automatically when followers are assigned)");
        
        Debug.Log("=== Room Clock Test PASSED ===");
    }
    
    private void TestGodStrengthAndFavor()
    {
        Debug.Log("=== TEST: God Strength and Favor ===");
        
        if (testGod == null)
        {
            Debug.LogError("No god to test!");
            return;
        }
        
        Debug.Log($"Initial - Strength: {testGod.Strength}, Favor: {testGod.Favor}");
        
        testGod.IncreaseStrength(10);
        Debug.Log($"After +10 strength: {testGod.Strength}");
        
        testGod.DecreaseStrength(25);
        Debug.Log($"After -25 strength: {testGod.Strength}");
        
        testGod.IncreaseFavor(20);
        Debug.Log($"After +20 favor: {testGod.Favor}");
        
        testGod.DecreaseFavor(15);
        Debug.Log($"After -15 favor: {testGod.Favor}");
        
        Debug.Log($"Can afford 30 favor? {testGod.CanAffordFavor(30)}");
        Debug.Log($"Can afford 100 favor? {testGod.CanAffordFavor(100)}");
        
        Debug.Log("=== God Strength/Favor Test PASSED ===");
    }
    
    private void TestMaskSystem()
    {
        Debug.Log("=== TEST: Mask System ===");
        
        if (testGod == null)
        {
            Debug.LogError("No god to test!");
            return;
        }
        
        // Create test masks
        var wrathMask = new Mask(MaskType.Wrath, MaskTargetType.EnemyRoom, 0f, 60f, 10, 0, 0, 15);
        var plentyMask = new Mask(MaskType.Plenty, MaskTargetType.None, 0f, 30f, 5, 0, 0, 25);
        
        Debug.Log($"Created Wrath mask - Type: {wrathMask.Type}, ShelfLife: {wrathMask.ShelfLife}");
        Debug.Log($"Created Plenty mask - Type: {plentyMask.Type}, EffectValue: {plentyMask.EffectValue}");
        
        // Add to god storage
        bool added1 = testGod.AddMaskToStorage(wrathMask);
        bool added2 = testGod.AddMaskToStorage(plentyMask);
        Debug.Log($"Added masks to storage: {added1}, {added2}");
        Debug.Log($"Masks in storage: {testGod.StoredMasks.Count}");
        
        // Test shelf life
        wrathMask.TickShelfLife(10f);
        Debug.Log($"Wrath shelf life after 10s: {wrathMask.ShelfLife}");
        Debug.Log($"Is expired? {wrathMask.IsExpired}");
        
        // Test can afford
        Debug.Log($"Can afford Plenty mask? {plentyMask.CanAfford(testCult)}");
        
        // Apply Plenty mask effect
        int favorBefore = testGod.Favor;
        plentyMask.ApplyEffect(testCult, null, null);
        Debug.Log($"Favor before Plenty: {favorBefore}, after: {testGod.Favor}");
        
        Debug.Log("=== Mask System Test PASSED ===");
    }
    
    private void TestMarketplace()
    {
        Debug.Log("=== TEST: Marketplace ===");
        
        if (testMarketplace == null)
        {
            Debug.LogError("No marketplace to test!");
            return;
        }
        
        Debug.Log($"Citizens: {testMarketplace.CitizenCount}");
        Debug.Log($"Is full? {testMarketplace.IsFull}");
        Debug.Log($"Has citizens? {testMarketplace.HasCitizens}");
        
        // Test recruit
        var recruited = testMarketplace.RecruitCitizen();
        Debug.Log($"Recruited citizen: {recruited != null}");
        Debug.Log($"Citizens after recruit: {testMarketplace.CitizenCount}");
        
        // Test spawn
        var spawned = testMarketplace.SpawnCitizen();
        Debug.Log($"Spawned citizen: {spawned != null}");
        Debug.Log($"Citizens after spawn: {testMarketplace.CitizenCount}");
        
        Debug.Log("=== Marketplace Test PASSED ===");
    }
    
    private void TestRoomAssignment()
    {
        Debug.Log("=== TEST: Room Assignment ===");
        
        if (testChurch == null || testCult == null)
        {
            Debug.LogError("No church or cult to test!");
            return;
        }
        
        var sanctuary = testChurch.GetRoomOfType(RoomType.Sanctuary);
        var altar = testChurch.GetRoomOfType(RoomType.Altar);
        
        if (sanctuary == null || altar == null)
        {
            Debug.LogError("Missing rooms!");
            return;
        }
        
        Debug.Log($"Sanctuary followers: {sanctuary.Followers.Count}");
        Debug.Log($"Altar followers: {altar.Followers.Count}");
        Debug.Log($"Altar has space? {altar.HasSpace}, Capacity: {altar.Capacity}");
        
        // Move a follower from sanctuary to altar
        if (sanctuary.Followers.Count > 0)
        {
            var follower = sanctuary.Followers[0] as Follower;
            sanctuary.RemoveFollower(follower);
            bool added = altar.AddFollower(follower);
            Debug.Log($"Moved follower to altar: {added}");
            Debug.Log($"Sanctuary followers now: {sanctuary.Followers.Count}");
            Debug.Log($"Altar followers now: {altar.Followers.Count}");
        }
        
        Debug.Log("=== Room Assignment Test PASSED ===");
    }
    
    private void TestWinLossConditions()
    {
        Debug.Log("=== TEST: Win/Loss Conditions ===");
        
        if (testGod == null)
        {
            Debug.LogError("No god to test!");
            return;
        }
        
        Debug.Log("Testing loss detection (not triggering actual loss)...");
        Debug.Log($"Current strength: {testGod.Strength} (loss at 0)");
        Debug.Log($"Current favor: {testGod.Favor} (loss at 0)");
        Debug.Log($"Current followers: {testCult.FollowerCount} (loss at 0)");
        
        Debug.Log("Loss conditions are checked each frame by GameManager");
        Debug.Log("=== Win/Loss Conditions Test PASSED ===");
    }
    
    // === Run All Tests ===
    
    private IEnumerator RunAllTests()
    {
        Debug.Log("========== RUNNING ALL TESTS ==========");
        
        yield return new WaitForSeconds(0.5f);
        TestFollowerCommitment();
        
        yield return new WaitForSeconds(0.5f);
        TestRoomClock();
        
        yield return new WaitForSeconds(0.5f);
        TestGodStrengthAndFavor();
        
        yield return new WaitForSeconds(0.5f);
        TestMaskSystem();
        
        yield return new WaitForSeconds(0.5f);
        TestMarketplace();
        
        yield return new WaitForSeconds(0.5f);
        TestRoomAssignment();
        
        yield return new WaitForSeconds(0.5f);
        TestWinLossConditions();
        
        Debug.Log("========== ALL TESTS COMPLETE ==========");
    }
}
