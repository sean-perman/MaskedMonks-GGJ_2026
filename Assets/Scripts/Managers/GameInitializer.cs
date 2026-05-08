using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Initializes the game with both cults, churches, rooms, and followers.
/// Attach this to the GameManager or a dedicated setup object.
/// 
/// PREFAB USAGE: Assign prefabs in the inspector. If a prefab is null,
/// the system will fall back to programmatic creation (legacy behavior).
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float roomWidth = 2.2f;
    [SerializeField] private float roomHeight = 1.6f;
    [SerializeField] private float roomSpacing = 0.08f;
    // Horizontal distance between the two cult roots (tuned to match scene)
    [SerializeField] private float churchSeparation = 3.0f; // Distance between churches
    
    [Header("Starting Configuration")]
    [SerializeField] private int startingFollowers = 3;
    [SerializeField] private int startingMaxMoney = 10;
    // Note: God strength, favor, and money now come from GameConfig
    
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    
    [Header("Prefabs - Core Objects")]
    [Tooltip("Prefab for the cult container object")]
    [SerializeField] private GameObject cultPrefab;
    
    [Tooltip("Prefab for the god object (should have God component)")]
    [SerializeField] private GameObject godPrefab;
    
    [Tooltip("Prefab for the church container object")]
    [SerializeField] private GameObject churchPrefab;
    
    [Tooltip("Prefab for follower objects (should have Follower component)")]
    [SerializeField] private GameObject followerPrefab;
    
    [Header("Prefabs - Rooms")]
    [Tooltip("Prefab for Sanctuary room")]
    [SerializeField] private GameObject sanctuaryRoomPrefab;
    
    [Tooltip("Prefab for Altar room")]
    [SerializeField] private GameObject altarRoomPrefab;
    
    [Tooltip("Prefab for Pews room")]
    [SerializeField] private GameObject pewsRoomPrefab;
    
    [Tooltip("Prefab for Mission room")]
    [SerializeField] private GameObject missionRoomPrefab;
    
    [Tooltip("Prefab for Ritual Hall room")]
    [SerializeField] private GameObject ritualHallRoomPrefab;
    
    [Tooltip("Prefab for Workshop room")]
    [SerializeField] private GameObject workshopRoomPrefab;
    
    [Tooltip("Prefab for Fundraising room")]
    [SerializeField] private GameObject fundraisingRoomPrefab;
    
    [Tooltip("Prefab for Lightning Ritual room")]
    [SerializeField] private GameObject lightningRitualRoomPrefab;
    
    [Tooltip("Prefab for Flood Ritual room")]
    [SerializeField] private GameObject floodRitualRoomPrefab;
    
    [Tooltip("Prefab for Shield Ritual room")]
    [SerializeField] private GameObject shieldRitualRoomPrefab;

    [Tooltip("Prefab for Sacrificial Altar room")]
    [SerializeField] private GameObject sacrificialAltarRoomPrefab;

    [Tooltip("Prefab for empty buildable slot")]
    [SerializeField] private GameObject emptySlotPrefab;
    
    [Header("Prefabs - UI & Visuals")]
    [Tooltip("Prefab for the marketplace")]
    [SerializeField] private GameObject marketplacePrefab;
    
    [Tooltip("Prefab for the background")]
    [SerializeField] private GameObject backgroundPrefab;

    [Tooltip("Sprite to use for the background (e.g. Assets/Art/Sprites/Background.png)")]
    [SerializeField] private Sprite backgroundSprite;
    
    [Tooltip("Prefab for player cursor visual")]
    [SerializeField] private GameObject cursorPrefab;
    
    [Tooltip("Prefab for player controller")]
    [SerializeField] private GameObject playerControllerPrefab;
    
    [Tooltip("Prefab for controls menu")]
    [SerializeField] private GameObject controlsMenuPrefab;
    
    // Created objects
    private Cult cult1;
    private Cult cult2;
    private PlayerController player1Controller;
    private PlayerController player2Controller;
    
    public Cult Cult1 => cult1;
    public Cult Cult2 => cult2;
    public PlayerController Player1Controller => player1Controller;
    public PlayerController Player2Controller => player2Controller;
    
    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }
        
        InitializeGame();
    }
    
    public void InitializeGame()
    {
        // Create background
        CreateBackground();
        
        // Create marketplace
        CreateMarketplace();
        
        // Create both cults at positions matching the tuned scene layout.
        // X values are taken from the inspector for Cult 1 and Cult 2.
        float cultBaseY = 0.6f;
        cult1 = CreateCult("Cult 1", new Vector3(-5.23f, cultBaseY, 0f), true);
        cult2 = CreateCult("Cult 2", new Vector3(5.8f, cultBaseY, 0f), false);
        
        // Create player controllers
        player1Controller = CreatePlayerController(0, cult1);
        player2Controller = CreatePlayerController(1, cult2);
        
        // Link player controllers to cults (for selected room immunity)
        cult1.SetPlayerController(player1Controller);
        cult2.SetPlayerController(player2Controller);
        
        // Register with GameManager
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }
        if (gameManager != null)
        {
            gameManager.SetCults(cult1, cult2);
            Debug.Log("Game initialized with both cults!");
        }
        else
        {
            Debug.LogWarning("GameManager not found! Cults not registered.");
        }
        
        // Create cursor visuals
        CreateCursorVisual(player1Controller, "Player 1 Cursor", new Color(0.3f, 0.7f, 1f));
        CreateCursorVisual(player2Controller, "Player 2 Cursor", new Color(1f, 0.5f, 0.3f));
        
        // Create controls menu
        CreateControlsMenu();
        
        // Create game over screen
        CreateGameOverScreen();
        
        // Create room info panels for each player
        CreateRoomInfoPanels();
    }
    
    private void CreateGameOverScreen()
    {
        var gameOverObj = new GameObject("GameOverScreen");
        var gameOverScreen = gameOverObj.AddComponent<GameOverScreen>();
        
        // Register with GameManager
        if (gameManager != null)
        {
            gameManager.SetGameOverScreen(gameOverScreen);
        }
        
        Debug.Log("Game Over Screen created.");
    }
    
    private void CreateBackground()
    {
        GameObject bgObj;
        
        if (backgroundPrefab != null)
        {
            bgObj = Instantiate(backgroundPrefab);
            bgObj.name = "Background";
            bgObj.transform.position = new Vector3(0, 0, 10);

            var sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = bgObj.AddComponent<SpriteRenderer>();
            }

            Sprite bgSprite = backgroundSprite;

            if (bgSprite == null)
            {
                bgSprite = Resources.Load<Sprite>("Background");
            }

            if (bgSprite != null)
            {
                sr.sortingOrder = -100;
                sr.sprite = bgSprite;

                float targetHeight = 20f;
                float spriteHeight = bgSprite.bounds.size.y;
                float scale = targetHeight / spriteHeight;
                bgObj.transform.localScale = new Vector3(scale, scale, 1f);

                Debug.Log("Loaded background sprite for backgroundPrefab");
            }
        }
        else
        {
            // Prefer explicit background sprite reference, then fall back to Resources
            Sprite bgSprite = backgroundSprite;
            if (bgSprite == null)
            {
                bgSprite = Resources.Load<Sprite>("Sprites/Background");
            }
            if (bgSprite == null)
            {
                bgSprite = Resources.Load<Sprite>("Background");
            }
            
            bgObj = new GameObject("Background");
            bgObj.transform.position = new Vector3(0, 0, 10); // Behind everything
            
            var sr = bgObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -100;
            
            if (bgSprite != null)
            {
                // Use the provided Background.png sprite as background
                sr.sprite = bgSprite;
                
                // Scale to cover the screen while maintaining aspect ratio
                float targetHeight = 20f; // Adjust based on camera size
                float spriteHeight = bgSprite.bounds.size.y;
                float scale = targetHeight / spriteHeight;
                bgObj.transform.localScale = new Vector3(scale, scale, 1f);
                
                Debug.Log("Loaded background sprite as background");
            }
            else
            {
                // Fallback: Create gradient texture if Background.png not found
                Debug.LogWarning("Background.png not found in Resources. Using gradient fallback.");
                int width = 64;
                int height = 64;
                Texture2D tex = new Texture2D(width, height);
            
            Color topColor = new Color(0.15f, 0.1f, 0.25f); // Dark purple
            Color bottomColor = new Color(0.05f, 0.05f, 0.1f); // Near black
            
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                Color color = Color.Lerp(bottomColor, topColor, t);
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }
                tex.Apply();
                tex.filterMode = FilterMode.Bilinear;
                
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1f);
                bgObj.transform.localScale = new Vector3(50f, 30f, 1f); // Cover screen
            }
        }
    }
    
    private void CreateMarketplace()
    {
        GameObject marketObj;
        
        if (marketplacePrefab != null)
        {
            marketObj = Instantiate(marketplacePrefab);
            marketObj.name = "Marketplace";
            marketObj.transform.position = new Vector3(0, -8f, 0);
            
            // Ensure Marketplace component exists
            if (marketObj.GetComponent<Marketplace>() == null)
            {
                marketObj.AddComponent<Marketplace>();
            }
        }
        else
        {
            // Fallback: programmatic creation
            marketObj = new GameObject("Marketplace");
            marketObj.transform.position = new Vector3(0, -8f, 0); // Bottom center
            marketObj.AddComponent<Marketplace>();
            
            // Add visual representation
            var sr = marketObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.4f, 0.3f, 0.2f, 0.8f);
            sr.transform.localScale = new Vector3(6f, 2f, 1f);
            sr.sortingOrder = -1;
        }
        
        Debug.Log("Marketplace created");
    }
    
    private Cult CreateCult(string cultName, Vector3 position, bool isPlayer1)
    {
        // Create cult root object
        GameObject cultObj;
        Cult cult;
        
        if (cultPrefab != null)
        {
            cultObj = Instantiate(cultPrefab);
            cultObj.name = cultName;
            cult = cultObj.GetComponent<Cult>();
            if (cult == null) cult = cultObj.AddComponent<Cult>();
        }
        else
        {
            cultObj = new GameObject(cultName);
            cult = cultObj.AddComponent<Cult>();
        }
        cultObj.transform.position = position;
        
        // Create church
        var church = CreateChurch(cultObj.transform, isPlayer1);
        
        // Create god
        var god = CreateGod(cultObj.transform);
        
        // Set references using reflection or public setters
        // Since cult.god and cult.church are public fields, we can set them directly
        cult.god = god;
        cult.church = church;
        
        // Set max money cap and add starting money from GameConfig
        cult.IncreaseMaxMoney(startingMaxMoney - 10); // Default is 10, so add the difference
        cult.AddMoney(GameConfig.Instance.godStartingMoney);
        
        // Create starting rooms
        CreateStartingRooms(church, isPlayer1);
        
        // Create starting followers
        CreateStartingFollowers(cult, church);
        
        return cult;
    }
    
    private Church CreateChurch(Transform parent, bool isPlayer1)
    {
        GameObject churchObj;
        Church church;
        
        if (churchPrefab != null)
        {
            churchObj = Instantiate(churchPrefab, parent);
            churchObj.name = "Church";
            church = churchObj.GetComponent<Church>();
            if (church == null) church = churchObj.AddComponent<Church>();
        }
        else
        {
            churchObj = new GameObject("Church");
            churchObj.transform.SetParent(parent);
            church = churchObj.AddComponent<Church>();
        }
        churchObj.transform.localPosition = Vector3.zero;

        // Add random church building sprite
        AddChurchSprite(churchObj.transform, isPlayer1);

        return church;
    }
    
    private God CreateGod(Transform parent)
    {
        GameObject godObj;
        God god;

        if (godPrefab != null)
        {
            godObj = Instantiate(godPrefab, parent);
            godObj.name = "God";
            god = godObj.GetComponent<God>();
            if (god == null) god = godObj.AddComponent<God>();
        }
        else
        {
            godObj = new GameObject("God");
            godObj.transform.SetParent(parent);
            god = godObj.AddComponent<God>();
        }
        godObj.transform.localPosition = new Vector3(0, 5f, 0); // Position above church

        // Add GodVisual if not present
        if (godObj.GetComponent<GodVisual>() == null)
        {
            godObj.AddComponent<GodVisual>();
        }

        // Add random god sprite
        AddGodSprite(godObj.transform);

        // Initialize with starting stats from GameConfig
        god.Initialize();

        return god;
    }

    /// <summary>
    /// Adds a random god sprite to the god object.
    /// Sprites should be located in Resources/gods/ folder.
    /// Positioned to the left of the health bars/UI.
    /// </summary>
    private void AddGodSprite(Transform godTransform)
    {
        // Load all god sprites from Resources/gods folder
        Sprite[] godSprites = Resources.LoadAll<Sprite>("gods");

        if (godSprites == null || godSprites.Length == 0)
        {
            Debug.LogWarning("No god sprites found in Resources/gods/. Please move sprites from Assets/Art/Sprites/gods to Assets/Resources/gods/");
            return;
        }

        // Pick a random sprite
        Sprite randomSprite = godSprites[Random.Range(0, godSprites.Length)];

        // Create sprite child object
        GameObject spriteObj = new GameObject("God Sprite");
        spriteObj.transform.SetParent(godTransform);
        // Position to the left of the health bars (which are at x=0, with bar width=2)
        // Health bar extends from x=-1 to x=1, so place sprite at x=-2
        spriteObj.transform.localPosition = new Vector3(-2f, 0.2f, 0f);
        spriteObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f); // Scale down if needed

        // Add sprite renderer
        SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
        sr.sprite = randomSprite;
        sr.sortingOrder = 15; // Render in front of UI elements (UI is at 10-12)

        // Add floating effect
        spriteObj.AddComponent<FloatingEffect>();

        Debug.Log($"Added god sprite: {randomSprite.name}");
    }

    /// <summary>
    /// Adds a random church building sprite to the church object.
    /// Sprites should be located in Resources/churchs/ folder.
    /// Positioned behind all rooms and UI elements.
    /// </summary>
    private void AddChurchSprite(Transform churchTransform, bool isPlayer1)
    {
        // Load all church sprites from Resources/churchs folder
        Sprite[] churchSprites = Resources.LoadAll<Sprite>("churchs");

        if (churchSprites == null || churchSprites.Length == 0)
        {
            Debug.LogWarning("No church sprites found in Resources/churchs/. Please move sprites from Assets/Art/Sprites/churchs to Assets/Resources/churchs/");
            return;
        }

        // Pick a random sprite
        Sprite randomSprite = churchSprites[Random.Range(0, churchSprites.Length)];

        // Create sprite child object
        GameObject spriteObj = new GameObject("Church Building Sprite");
        spriteObj.transform.SetParent(churchTransform);
        // Position at center to align with tightly packed room grid
        spriteObj.transform.localPosition = new Vector3(0f, 0.0f, 0f);

        // Add sprite renderer
        SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
        sr.sprite = randomSprite;
        sr.sortingOrder = -10; // Render behind all rooms and UI (rooms are at 0+)

        // Use a fixed scale tuned in the editor so the 3x4 room grid
        // fits neatly inside the dark interior of the church sprite.
        const float tunedScaleX = 1.0416f;
        const float tunedScaleY = 1.1456f;

        // Flip sprite horizontally for Player 2 (right side)
        float xScale = isPlayer1 ? tunedScaleX : -tunedScaleX;
        spriteObj.transform.localScale = new Vector3(xScale, tunedScaleY, 1f);

        Debug.Log($"Added church building sprite: {randomSprite.name} (flipped: {!isPlayer1})");
    }

    private void CreateStartingRooms(Church church, bool isPlayer1)
    {
        // Get church transform for positioning
        Transform churchTransform = church.transform;

        // Calculate grid offset so rooms are tightly packed and centered within the church building
            float gridWidth = church.GridWidth * (roomWidth + roomSpacing);
            float gridHeight = church.GridHeight * (roomHeight + roomSpacing);

            // Center the grid inside the church and move it downwards so rooms
            // sit inside the dark interior. The Y offset is tuned to match the
            // layout you set up in the scene.
            float yOffsetDown = 1.6f;

            Vector3 gridOffset = new Vector3(
                -gridWidth / 2f + roomWidth / 2f,
                -gridHeight / 2f + roomHeight / 2f - yOffsetDown,
                0f
            );

        // Create all rooms at their designated positions
        // Rooms with starting level 0 will appear but cost buildCost to build the first level

        // Bottom row (y=0): Core rooms
        CreateRoomFromPrefab<SanctuaryRoom>(church, new Vector2Int(0, 0), churchTransform, gridOffset, sanctuaryRoomPrefab);
        CreateRoomFromPrefab<AltarRoom>(church, new Vector2Int(1, 0), churchTransform, gridOffset, altarRoomPrefab);
        CreateRoomFromPrefab<PewsRoom>(church, new Vector2Int(2, 0), churchTransform, gridOffset, pewsRoomPrefab);

        // Second row (y=1): Support rooms
        CreateRoomFromPrefab<WorkshopRoom>(church, new Vector2Int(0, 1), churchTransform, gridOffset, workshopRoomPrefab);
        CreateRoomFromPrefab<MissionRoom>(church, new Vector2Int(1, 1), churchTransform, gridOffset, missionRoomPrefab);
        CreateRoomFromPrefab<FundraisingRoom>(church, new Vector2Int(2, 1), churchTransform, gridOffset, fundraisingRoomPrefab);

        // Third row (y=2): Ritual rooms for generating masks
        CreateRoomFromPrefab<LightningRitualRoom>(church, new Vector2Int(0, 2), churchTransform, gridOffset, lightningRitualRoomPrefab);
        CreateRoomFromPrefab<RitualHallRoom>(church, new Vector2Int(1, 2), churchTransform, gridOffset, ritualHallRoomPrefab);
        CreateRoomFromPrefab<ShieldRitualRoom>(church, new Vector2Int(2, 2), churchTransform, gridOffset, shieldRitualRoomPrefab);

        // Fourth row (y=3): Advanced rooms
        CreateRoomFromPrefab<SacrificialAltarRoom>(church, new Vector2Int(0, 3), churchTransform, gridOffset, sacrificialAltarRoomPrefab);
        CreateRoomFromPrefab<FloodRitualRoom>(church, new Vector2Int(1, 3), churchTransform, gridOffset, floodRitualRoomPrefab);

        // Create empty slots for remaining positions (if any)
        for (int x = 0; x < church.GridWidth; x++)
        {
            for (int y = 0; y < church.GridHeight; y++)
            {
                var pos = new Vector2Int(x, y);
                if (church.GetRoomAt(pos) == null)
                {
                    CreateEmptySlot(church, pos, churchTransform, gridOffset);
                }
            }
        }
    }
    
    private T CreateRoomFromPrefab<T>(Church church, Vector2Int gridPos, Transform parent, Vector3 gridOffset, GameObject prefab) where T : Room
    {
        string roomName = typeof(T).Name.Replace("Room", "");
        GameObject roomObj;
        T room;
        
        if (prefab != null)
        {
            roomObj = Instantiate(prefab, parent);
            roomObj.name = $"{roomName} [{gridPos.x},{gridPos.y}]";
            room = roomObj.GetComponent<T>();
            if (room == null) room = roomObj.AddComponent<T>();
            
            // Ensure RoomVisual exists
            if (roomObj.GetComponent<RoomVisual>() == null)
            {
                roomObj.AddComponent<RoomVisual>();
            }
        }
        else
        {
            // Fallback: programmatic creation
            roomObj = new GameObject($"{roomName} [{gridPos.x},{gridPos.y}]");
            roomObj.transform.SetParent(parent);
            room = roomObj.AddComponent<T>();
            roomObj.AddComponent<RoomVisual>();
        }
        
        // Calculate world position
        Vector3 localPos = new Vector3(
            gridPos.x * (roomWidth + roomSpacing),
            gridPos.y * (roomHeight + roomSpacing),
            0
        ) + gridOffset;
        roomObj.transform.localPosition = localPos;
        
        // Register with church
        church.AddRoom(room, gridPos);
        
        return room;
    }
    
    /// <summary>
    /// Gets the appropriate prefab for a room type. Used for runtime room creation.
    /// </summary>
    public GameObject GetRoomPrefab(RoomType type)
    {
        return type switch
        {
            RoomType.Sanctuary => sanctuaryRoomPrefab,
            RoomType.Altar => altarRoomPrefab,
            RoomType.Pews => pewsRoomPrefab,
            RoomType.Mission => missionRoomPrefab,
            RoomType.WrathRitualHall => ritualHallRoomPrefab,
            RoomType.Workshop => workshopRoomPrefab,
            RoomType.Fundraising => fundraisingRoomPrefab,
            RoomType.LightningRitual => lightningRitualRoomPrefab,
            RoomType.FloodRitual => floodRitualRoomPrefab,
            RoomType.ShieldRitual => shieldRitualRoomPrefab,
            RoomType.SacrificialAltar => sacrificialAltarRoomPrefab,
            _ => null
        };
    }
    
    private void CreateEmptySlot(Church church, Vector2Int gridPos, Transform parent, Vector3 gridOffset)
    {
        GameObject slotObj;
        RoomSlot slot;
        
        if (emptySlotPrefab != null)
        {
            slotObj = Instantiate(emptySlotPrefab, parent);
            slotObj.name = $"Empty Slot [{gridPos.x},{gridPos.y}]";
            slot = slotObj.GetComponent<RoomSlot>();
            if (slot == null) slot = slotObj.AddComponent<RoomSlot>();
        }
        else
        {
            // Fallback: programmatic creation
            slotObj = new GameObject($"Empty Slot [{gridPos.x},{gridPos.y}]");
            slotObj.transform.SetParent(parent);
            slot = slotObj.AddComponent<RoomSlot>();
            
            // Add visual indicator
            var sr = slotObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            sr.transform.localScale = new Vector3(roomWidth * 0.9f, roomHeight * 0.9f, 1f);
        }
        
        // Calculate world position
        Vector3 localPos = new Vector3(
            gridPos.x * (roomWidth + roomSpacing),
            gridPos.y * (roomHeight + roomSpacing),
            0
        ) + gridOffset;
        slotObj.transform.localPosition = localPos;
        
        // Initialize slot
        slot.Initialize(church, gridPos);
    }
    
    private void CreateStartingFollowers(Cult cult, Church church)
    {
        var sanctuary = church.GetRoomOfType(RoomType.Sanctuary);
        
        for (int i = 0; i < startingFollowers; i++)
        {
            Follower follower = CreateFollower(cult, $"Follower {i + 1}");
            
            // Add to cult (this also tries to place in sanctuary)
            cult.AddFollower(follower);
            
            // If sanctuary is full, just leave them unassigned
            if (sanctuary != null && sanctuary.HasSpace)
            {
                sanctuary.AddFollower(follower);
            }
        }
        
        Debug.Log($"Created {startingFollowers} followers for {cult.name}");
    }
    
    /// <summary>
    /// Creates a follower using prefab if available. Public for runtime follower creation.
    /// </summary>
    public Follower CreateFollower(Cult cult, string name = "Follower")
    {
        GameObject followerObj;
        Follower follower;
        
        if (followerPrefab != null)
        {
            followerObj = Instantiate(followerPrefab, cult.transform);
            followerObj.name = name;
            follower = followerObj.GetComponent<Follower>();
            if (follower == null) follower = followerObj.AddComponent<Follower>();
        }
        else
        {
            followerObj = new GameObject(name);
            followerObj.transform.SetParent(cult.transform);
            follower = followerObj.AddComponent<Follower>();
        }
        
        follower.Initialize(cult);
        return follower;
    }
    
    private PlayerController CreatePlayerController(int playerIndex, Cult cult)
    {
        GameObject controllerObj;
        PlayerController controller;
        
        if (playerControllerPrefab != null)
        {
            controllerObj = Instantiate(playerControllerPrefab, transform);
            controllerObj.name = $"Player {playerIndex + 1} Controller";
            controller = controllerObj.GetComponent<PlayerController>();
            if (controller == null) controller = controllerObj.AddComponent<PlayerController>();
        }
        else
        {
            controllerObj = new GameObject($"Player {playerIndex + 1} Controller");
            controllerObj.transform.SetParent(transform);
            controller = controllerObj.AddComponent<PlayerController>();
        }
        
        controller.Initialize(playerIndex, cult);
        return controller;
    }
    
    private void CreateCursorVisual(PlayerController controller, string name, Color tint)
    {
        GameObject cursorObj;
        CursorVisual cursor;
        
        if (cursorPrefab != null)
        {
            cursorObj = Instantiate(cursorPrefab, transform);
            cursorObj.name = name;
            cursor = cursorObj.GetComponent<CursorVisual>();
            if (cursor == null) cursor = cursorObj.AddComponent<CursorVisual>();
        }
        else
        {
            cursorObj = new GameObject(name);
            cursorObj.transform.SetParent(transform);
            cursor = cursorObj.AddComponent<CursorVisual>();
        }
        
        cursor.SetController(controller);
    }
    
    private void CreateControlsMenu()
    {
        GameObject menuObj;
        ControlsMenu menu;
        
        if (controlsMenuPrefab != null)
        {
            menuObj = Instantiate(controlsMenuPrefab, transform);
            menuObj.name = "Controls Menu";
            menu = menuObj.GetComponent<ControlsMenu>();
            if (menu == null) menu = menuObj.AddComponent<ControlsMenu>();
        }
        else
        {
            menuObj = new GameObject("Controls Menu");
            menuObj.transform.SetParent(transform);
            menu = menuObj.AddComponent<ControlsMenu>();
        }
        
        menu.SetControllers(player1Controller, player2Controller);
    }
    
    private void CreateRoomInfoPanels()
    {
        // Player 1 panel (left side)
        var panel1Obj = new GameObject("RoomInfoPanel_Player1");
        panel1Obj.transform.SetParent(transform);
        var panel1 = panel1Obj.AddComponent<RoomInfoPanel>();
        panel1.SetController(player1Controller, true); // left side
        
        // Player 2 panel (right side)
        var panel2Obj = new GameObject("RoomInfoPanel_Player2");
        panel2Obj.transform.SetParent(transform);
        var panel2 = panel2Obj.AddComponent<RoomInfoPanel>();
        panel2.SetController(player2Controller, false); // right side
        
        Debug.Log("Room Info Panels created for both players.");
    }
    
    private Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
