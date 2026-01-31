using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Initializes the game with both cults, churches, rooms, and followers.
/// Attach this to the GameManager or a dedicated setup object.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float roomWidth = 1.8f;
    [SerializeField] private float roomHeight = 1.4f;
    [SerializeField] private float roomSpacing = 0.15f;
    [SerializeField] private float churchSeparation = 12f; // Distance between churches
    
    [Header("Starting Configuration")]
    [SerializeField] private int startingFollowers = 5;
    [SerializeField] private int startingGodStrength = 100;
    [SerializeField] private int startingGodFavor = 50;
    [SerializeField] private float startingMoney = 100f;
    
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    
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
        
        // Create both cults
        cult1 = CreateCult("Cult 1", new Vector3(-churchSeparation / 2, 0, 0), true);
        cult2 = CreateCult("Cult 2", new Vector3(churchSeparation / 2, 0, 0), false);
        
        // Create player controllers
        player1Controller = CreatePlayerController(0, cult1);
        player2Controller = CreatePlayerController(1, cult2);
        
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
    }
    
    private void CreateBackground()
    {
        var bgObj = new GameObject("Background");
        bgObj.transform.position = new Vector3(0, 0, 10); // Behind everything
        
        var sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -100;
        
        // Create gradient texture
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
        sr.transform.localScale = new Vector3(50f, 30f, 1f); // Cover screen
    }
    
    private void CreateMarketplace()
    {
        var marketObj = new GameObject("Marketplace");
        marketObj.transform.position = new Vector3(0, -8f, 0); // Bottom center
        var marketplace = marketObj.AddComponent<Marketplace>();
        
        // Add visual representation
        var sr = marketObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.4f, 0.3f, 0.2f, 0.8f);
        sr.transform.localScale = new Vector3(6f, 2f, 1f);
        sr.sortingOrder = -1;
        
        Debug.Log("Marketplace created");
    }
    
    private Cult CreateCult(string cultName, Vector3 position, bool isPlayer1)
    {
        // Create cult root object
        var cultObj = new GameObject(cultName);
        cultObj.transform.position = position;
        var cult = cultObj.AddComponent<Cult>();
        
        // Create church
        var church = CreateChurch(cultObj.transform, isPlayer1);
        
        // Create god
        var god = CreateGod(cultObj.transform);
        
        // Set references using reflection or public setters
        // Since cult.god and cult.church are public fields, we can set them directly
        cult.god = god;
        cult.church = church;
        
        // Add starting money
        cult.AddMoney(startingMoney);
        
        // Create starting rooms
        CreateStartingRooms(church, isPlayer1);
        
        // Create starting followers
        CreateStartingFollowers(cult, church);
        
        return cult;
    }
    
    private Church CreateChurch(Transform parent, bool isPlayer1)
    {
        var churchObj = new GameObject("Church");
        churchObj.transform.SetParent(parent);
        churchObj.transform.localPosition = Vector3.zero;
        var church = churchObj.AddComponent<Church>();
        
        return church;
    }
    
    private God CreateGod(Transform parent)
    {
        var godObj = new GameObject("God");
        godObj.transform.SetParent(parent);
        godObj.transform.localPosition = new Vector3(0, 5f, 0); // Position above church
        var god = godObj.AddComponent<God>();
        
        // Initialize with starting stats
        god.Initialize(startingGodStrength, startingGodFavor);
        
        return god;
    }
    
    private void CreateStartingRooms(Church church, bool isPlayer1)
    {
        // Get church transform for positioning
        Transform churchTransform = church.transform;
        
        // Calculate grid offset so rooms are centered under church
        float gridWidth = church.GridWidth * (roomWidth + roomSpacing);
        float gridHeight = church.GridHeight * (roomHeight + roomSpacing);
        Vector3 gridOffset = new Vector3(-gridWidth / 2 + roomWidth / 2, -gridHeight / 2 + roomHeight / 2, 0);
        
        // Bottom row (y=0): Core rooms
        // Create a Sanctuary at (0, 0)
        CreateRoom<SanctuaryRoom>(church, new Vector2Int(0, 0), churchTransform, gridOffset);
        
        // Create an Altar at (1, 0)
        CreateRoom<AltarRoom>(church, new Vector2Int(1, 0), churchTransform, gridOffset);
        
        // Create Pews at (2, 0)
        CreateRoom<PewsRoom>(church, new Vector2Int(2, 0), churchTransform, gridOffset);
        
        // Second row (y=1): Mission for converting citizens
        CreateRoom<MissionRoom>(church, new Vector2Int(1, 1), churchTransform, gridOffset);
        
        // Create empty slots for remaining positions
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
    
    private T CreateRoom<T>(Church church, Vector2Int gridPos, Transform parent, Vector3 gridOffset) where T : Room
    {
        string roomName = typeof(T).Name.Replace("Room", "");
        var roomObj = new GameObject($"{roomName} [{gridPos.x},{gridPos.y}]");
        roomObj.transform.SetParent(parent);
        
        // Calculate world position
        Vector3 localPos = new Vector3(
            gridPos.x * (roomWidth + roomSpacing),
            gridPos.y * (roomHeight + roomSpacing),
            0
        ) + gridOffset;
        roomObj.transform.localPosition = localPos;
        
        // Add room component
        var room = roomObj.AddComponent<T>();
        
        // Add visual
        roomObj.AddComponent<RoomVisual>();
        
        // Register with church
        church.AddRoom(room, gridPos);
        
        return room;
    }
    
    private void CreateEmptySlot(Church church, Vector2Int gridPos, Transform parent, Vector3 gridOffset)
    {
        var slotObj = new GameObject($"Empty Slot [{gridPos.x},{gridPos.y}]");
        slotObj.transform.SetParent(parent);
        
        // Calculate world position
        Vector3 localPos = new Vector3(
            gridPos.x * (roomWidth + roomSpacing),
            gridPos.y * (roomHeight + roomSpacing),
            0
        ) + gridOffset;
        slotObj.transform.localPosition = localPos;
        
        // Add empty slot component
        var slot = slotObj.AddComponent<RoomSlot>();
        slot.Initialize(church, gridPos);
        
        // Add visual indicator
        var sr = slotObj.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = slotObj.AddComponent<SpriteRenderer>();
        }
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        sr.transform.localScale = new Vector3(roomWidth * 0.9f, roomHeight * 0.9f, 1f);
    }
    
    private void CreateStartingFollowers(Cult cult, Church church)
    {
        var sanctuary = church.GetRoomOfType(RoomType.Sanctuary);
        
        for (int i = 0; i < startingFollowers; i++)
        {
            var followerObj = new GameObject($"Follower {i + 1}");
            followerObj.transform.SetParent(cult.transform);
            
            var follower = followerObj.AddComponent<Follower>();
            follower.Initialize(cult);
            
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
    
    private PlayerController CreatePlayerController(int playerIndex, Cult cult)
    {
        var controllerObj = new GameObject($"Player {playerIndex + 1} Controller");
        controllerObj.transform.SetParent(transform);
        
        var controller = controllerObj.AddComponent<PlayerController>();
        controller.Initialize(playerIndex, cult);
        
        return controller;
    }
    
    private void CreateCursorVisual(PlayerController controller, string name, Color tint)
    {
        var cursorObj = new GameObject(name);
        cursorObj.transform.SetParent(transform);
        
        var cursor = cursorObj.AddComponent<CursorVisual>();
        cursor.SetController(controller);
    }
    
    private void CreateControlsMenu()
    {
        var menuObj = new GameObject("Controls Menu");
        menuObj.transform.SetParent(transform);
        
        var menu = menuObj.AddComponent<ControlsMenu>();
        menu.SetControllers(player1Controller, player2Controller);
    }
    
    private Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
