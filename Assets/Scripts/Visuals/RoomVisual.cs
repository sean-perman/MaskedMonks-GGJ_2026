using UnityEngine;

/// <summary>
/// Visual representation of a room with follower count, capacity, progress, and level indicators.
/// Attach this to Room GameObjects for visual feedback.
/// 
/// Level is shown as circles in the top-right corner:
/// - Filled circles = current level
/// - Empty/greyed circles = potential upgrade slots
/// - Red outline on circles = damage levels
/// </summary>
[RequireComponent(typeof(Room))]
public class RoomVisual : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float roomWidth = 1.8f;
    [SerializeField] private float roomHeight = 1.4f;
    [SerializeField] private float followerIconSize = 0.25f;
    [SerializeField] private float levelPipSize = 0.12f;
    [SerializeField] private float levelPipSpacing = 0.04f;
    [SerializeField] private int maxLevelDisplay = 5;
    
    [Header("Colors")]
    [SerializeField] private Color sanctuaryColor = new Color(0.2f, 0.8f, 0.4f);
    [SerializeField] private Color altarColor = new Color(0.8f, 0.6f, 0.2f);
    [SerializeField] private Color pewsColor = new Color(0.6f, 0.6f, 0.8f);
    [SerializeField] private Color missionColor = new Color(0.3f, 0.5f, 0.8f);
    [SerializeField] private Color ritualColor = new Color(0.8f, 0.3f, 0.5f);
    [SerializeField] private Color workshopColor = new Color(0.7f, 0.5f, 0.3f);
    [SerializeField] private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f);
    [SerializeField] private Color targetColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color levelFilledColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color levelEmptyColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
    [SerializeField] private Color damageOutlineColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color orangeDamageColor = new Color(1f, 0.6f, 0.2f); // Orange damage outline
    [SerializeField] private Color redDamageColor = new Color(0.8f, 0.1f, 0.1f); // Red damage (slot unusable)
    [SerializeField] private Color repairBarColor = new Color(0.3f, 0.8f, 0.3f);
    
    [Header("Resource Bar Colors")]
    [SerializeField] private Color strengthBarColor = new Color(0.9f, 0.3f, 0.3f); // Red for strength
    [SerializeField] private Color favorBarColor = new Color(0.6f, 0.3f, 0.9f);    // Purple for favor
    [SerializeField] private Color moneyBarColor = new Color(1f, 0.85f, 0.2f);     // Gold for money
    [SerializeField] private Color maskBarColor = new Color(0.3f, 0.6f, 0.9f);     // Blue for masks
    [SerializeField] private Color blueprintBarColor = new Color(0.4f, 0.7f, 0.9f); // Light blue for blueprints
    [SerializeField] private Color followerBarColor = new Color(0.3f, 0.9f, 0.5f); // Green for followers
    [SerializeField] private Color defaultBarColor = Color.cyan;                    // Default cyan
    
    [Header("Resource Spawn Indicator")]
    [SerializeField] private float iconSpawnSize = 0.3f;
    [SerializeField] private float iconFloatHeight = 1.5f;
    [SerializeField] private float iconFloatDuration = 1.0f;
    
    private Room room;
    private SpriteRenderer backgroundSprite;
    private SpriteRenderer progressBar;
    private SpriteRenderer repairBar;
    private SpriteRenderer[] followerSlots;         // Square slot backgrounds
    private SpriteRenderer[] followerCircles;       // Circular pawn icons
    private SpriteRenderer[] followerDamageOutlines; // Damage indicator outlines
    private SpriteRenderer highlightBorder;
    private TextMesh labelText;
    private LevelPipVisual[] levelPips;
    
    private bool isHighlighted = false;
    private bool isTargeted = false;
    
    /// <summary>
    /// The Room this visual represents.
    /// </summary>
    public Room Room => room;
    
    private void Awake()
    {
        room = GetComponent<Room>();
        CreateVisuals();
        
        // Subscribe to resource generation events
        if (room != null)
        {
            room.OnResourceGenerated += OnResourceGenerated;
            room.OnMaskGenerated += OnMaskGenerated;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (room != null)
        {
            room.OnResourceGenerated -= OnResourceGenerated;
            room.OnMaskGenerated -= OnMaskGenerated;
        }
    }
    
    private void OnMaskGenerated(MaskType maskType)
    {
        SpawnMaskIndicator(maskType);
    }
    
    private void OnResourceGenerated(ResourceType resource, int amount)
    {
        SpawnResourceIndicator(resource, amount);
    }
    
    /// <summary>
    /// Overload to spawn a mask-specific indicator showing the mask shape.
    /// </summary>
    public void SpawnMaskIndicator(MaskType maskType)
    {
        var iconObj = new GameObject($"MaskIcon_{maskType}");
        iconObj.transform.SetParent(transform);
        iconObj.transform.localPosition = Vector3.zero;
        
        var iconRenderer = iconObj.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = CreateMaskSprite(maskType);
        iconRenderer.transform.localScale = new Vector3(iconSpawnSize * 1.5f, iconSpawnSize * 1.5f, 1f);
        iconRenderer.color = GetMaskTypeColor(maskType);
        iconRenderer.sortingOrder = 10;
        
        var floater = iconObj.AddComponent<FloatingResourceIcon>();
        floater.Initialize(iconFloatHeight, iconFloatDuration, 1);
    }
    
    private void SpawnResourceIndicator(ResourceType resource, int amount)
    {
        // For mask resources, skip the generic indicator - the specific mask indicator
        // is spawned separately via OnMaskGenerated/SpawnMaskIndicator
        if (resource == ResourceType.Mask)
            return;
            
        // This handles generic resource types (Strength, Favor, Money, Follower, Blueprint)
        var iconObj = new GameObject($"ResourceIcon_{resource}");
        iconObj.transform.SetParent(transform);
        iconObj.transform.localPosition = Vector3.zero;
        
        var iconRenderer = iconObj.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = CreateResourceIcon(resource);
        iconRenderer.transform.localScale = new Vector3(iconSpawnSize, iconSpawnSize, 1f);
        iconRenderer.color = GetResourceColor(resource);
        iconRenderer.sortingOrder = 10;
        
        // Add floating behavior
        var floater = iconObj.AddComponent<FloatingResourceIcon>();
        floater.Initialize(iconFloatHeight, iconFloatDuration, amount);
    }
    
    private Sprite CreateMaskSprite(MaskType maskType)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        float centerX = size / 2f;
        float centerY = size / 2f;
        
        // Clear texture first to fully transparent
        Color clearColor = new Color(0, 0, 0, 0);
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, clearColor);
        
        switch (maskType)
        {
            case MaskType.Strike:
                DrawStrikeMask(tex, size, centerX, centerY);
                break;
            case MaskType.Lightning:
                DrawLightningMask(tex, size, centerX, centerY);
                break;
            case MaskType.Flood:
                DrawFloodMask(tex, size, centerX, centerY);
                break;
            case MaskType.Shield:
                DrawShieldMask(tex, size, centerX, centerY);
                break;
            // Architecture masks - draw building shape
            case MaskType.ArchitectSanctuary:
            case MaskType.ArchitectAltar:
            case MaskType.ArchitectPews:
            case MaskType.ArchitectMission:
            case MaskType.ArchitectRitualHall:
            case MaskType.ArchitectWorkshop:
            case MaskType.ArchitectFundraising:
            case MaskType.ArchitectLightningRitual:
            case MaskType.ArchitectFloodRitual:
            case MaskType.ArchitectShieldRitual:
                DrawBlueprintMask(tex, size, centerX, centerY);
                break;
            default:
                DrawGenericMask(tex, size, centerX, centerY);
                break;
        }
        
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private void DrawBlueprintMask(Texture2D tex, int size, float cx, float cy)
    {
        // Simple building/house shape for blueprints
        int padding = 4;
        int roofHeight = size / 3;
        int bodyTop = padding + roofHeight;
        int bodyBottom = size - padding;
        int bodyLeft = padding + 2;
        int bodyRight = size - padding - 2;
        
        // Draw building body (rectangle)
        for (int x = bodyLeft; x <= bodyRight; x++)
        {
            for (int y = padding; y < bodyBottom; y++)
            {
                if (y >= bodyTop)
                    tex.SetPixel(x, y, Color.white);
            }
        }
        
        // Draw roof (triangle)
        for (int y = bodyTop; y >= padding; y--)
        {
            float progress = (float)(bodyTop - y) / roofHeight;
            int halfWidth = (int)((bodyRight - bodyLeft) / 2 * (1f - progress));
            int roofCenterX = (int)cx;
            for (int x = roofCenterX - halfWidth; x <= roofCenterX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, Color.white);
            }
        }
        
        // Draw door (small rectangle at bottom center)
        int doorWidth = 4;
        int doorHeight = 6;
        int doorLeft = (int)cx - doorWidth / 2;
        int doorBottom = padding;
        for (int x = doorLeft; x < doorLeft + doorWidth; x++)
        {
            for (int y = doorBottom; y < doorBottom + doorHeight; y++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                    tex.SetPixel(x, y, Color.clear); // Cut out the door
            }
        }
    }
    
    private void DrawStrikeMask(Texture2D tex, int size, float cx, float cy)
    {
        // Aggressive mask with angular features - like a fist/impact
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = (x - cx) / (size * 0.4f);
                float dy = (y - cy) / (size * 0.3f);
                bool inFace = dx * dx + dy * dy < 1f;
                
                // Angular eye slits
                float eyeY = cy + 2;
                bool inLeftEye = Mathf.Abs(x - (cx - 5)) < 4 && Mathf.Abs(y - eyeY) < 2;
                bool inRightEye = Mathf.Abs(x - (cx + 5)) < 4 && Mathf.Abs(y - eyeY) < 2;
                
                if (inFace && !inLeftEye && !inRightEye)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private void DrawLightningMask(Texture2D tex, int size, float cx, float cy)
    {
        // Lightning bolt shape
        float boltWidth = size * 0.15f;
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            float offsetX = Mathf.Sin(t * Mathf.PI * 2) * size * 0.2f;
            for (int x = 0; x < size; x++)
            {
                if (Mathf.Abs(x - cx - offsetX) < boltWidth)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private void DrawFloodMask(Texture2D tex, int size, float cx, float cy)
    {
        // Wave pattern
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float waveY = cy + Mathf.Sin((x / (float)size) * Mathf.PI * 3) * (size * 0.15f);
                if (y > waveY - size * 0.2f && y < waveY + size * 0.1f)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private void DrawShieldMask(Texture2D tex, int size, float cx, float cy)
    {
        // Shield shape (like a coat of arms)
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = Mathf.Abs(x - cx) / (size * 0.35f);
                float dyTop = (y - cy - size * 0.1f) / (size * 0.3f);
                float dyBot = (cy - y) / (size * 0.4f);
                
                bool inTop = dyTop < 0 && dx < 1f;
                bool inBot = dyBot < 0 && dyBot > -1f && dx < (1f + dyBot * 0.5f);
                
                if (inTop || inBot)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private void DrawGenericMask(Texture2D tex, int size, float cx, float cy)
    {
        // Basic oval mask with eye holes
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = (x - cx) / (size * 0.45f);
                float dy = (y - cy) / (size * 0.35f);
                bool inFace = dx * dx + dy * dy < 1f;
                
                float eyeY = cy + 2;
                float eyeRadius = 3f;
                bool inLeftEye = Vector2.Distance(new Vector2(x, y), new Vector2(cx - 5, eyeY)) < eyeRadius;
                bool inRightEye = Vector2.Distance(new Vector2(x, y), new Vector2(cx + 5, eyeY)) < eyeRadius;
                
                if (inFace && !inLeftEye && !inRightEye)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private Color GetMaskTypeColor(MaskType type)
    {
        return type switch
        {
            MaskType.Strike => new Color(1f, 0.4f, 0.2f),       // Orange-red
            MaskType.Lightning => new Color(1f, 1f, 0.3f),     // Yellow
            MaskType.Flood => new Color(0.3f, 0.5f, 1f),       // Blue
            MaskType.Shield => new Color(0.5f, 0.8f, 1f),      // Light blue
            MaskType.Smiting => new Color(1f, 0.3f, 0.3f),     // Red
            MaskType.Wrath => new Color(1f, 0.2f, 0.5f),       // Magenta
            MaskType.Whispers => new Color(0.5f, 0.3f, 0.8f),  // Purple
            MaskType.Sanctuary => new Color(0.3f, 0.8f, 0.5f), // Green
            MaskType.Plenty => new Color(1f, 0.85f, 0.3f),     // Gold
            MaskType.Sacrifice => new Color(0.8f, 0.2f, 0.2f), // Dark red
            // Architecture masks - all use a blueprint/construction color
            MaskType.ArchitectSanctuary => new Color(0.6f, 0.8f, 0.4f),    // Light green
            MaskType.ArchitectAltar => new Color(0.8f, 0.6f, 0.3f),        // Bronze
            MaskType.ArchitectPews => new Color(0.6f, 0.5f, 0.4f),         // Wood brown
            MaskType.ArchitectMission => new Color(0.4f, 0.6f, 0.8f),      // Sky blue
            MaskType.ArchitectRitualHall => new Color(0.7f, 0.4f, 0.7f),   // Purple
            MaskType.ArchitectWorkshop => new Color(0.7f, 0.7f, 0.5f),     // Khaki
            MaskType.ArchitectFundraising => new Color(0.9f, 0.8f, 0.2f),  // Gold
            MaskType.ArchitectLightningRitual => new Color(1f, 1f, 0.5f),  // Bright yellow
            MaskType.ArchitectFloodRitual => new Color(0.4f, 0.6f, 0.9f),  // Water blue
            MaskType.ArchitectShieldRitual => new Color(0.6f, 0.9f, 0.9f), // Cyan
            _ => Color.white
        };
    }
    
    private Sprite CreateResourceIcon(ResourceType resource)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        
        switch (resource)
        {
            case ResourceType.Strength:
                // Star shape for strength
                DrawStar(tex, size, center, radius - 2);
                break;
            case ResourceType.Favor:
                // Heart shape for favor
                DrawHeart(tex, size, center, radius - 2);
                break;
            case ResourceType.Money:
                // Coin shape (circle with inner circle)
                DrawCoin(tex, size, center, radius - 2);
                break;
            case ResourceType.Mask:
            case ResourceType.Blueprint:
                // Diamond shape for masks/blueprints
                DrawDiamond(tex, size, center, radius - 2);
                break;
            case ResourceType.Follower:
                // Person shape (circle + triangle)
                DrawPerson(tex, size, center, radius - 2);
                break;
            case ResourceType.Repair:
                // Wrench/tool shape for repairs
                DrawWrench(tex, size, center, radius - 2);
                break;
            default:
                // Simple circle
                DrawCircle(tex, size, center, radius - 2);
                break;
        }
        
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private void DrawStar(Texture2D tex, int size, Vector2 center, float radius)
    {
        // Clear texture
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, Color.clear);
        
        // Simple 4-point star using triangles
        int points = 4;
        float innerRadius = radius * 0.4f;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 p = new Vector2(x, y) - center;
                float angle = Mathf.Atan2(p.y, p.x);
                float dist = p.magnitude;
                float starAngle = Mathf.Repeat(angle * points / (2 * Mathf.PI), 1f);
                float starRadius = Mathf.Lerp(innerRadius, radius, Mathf.Abs(starAngle - 0.5f) * 2);
                if (dist < starRadius)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private void DrawHeart(Texture2D tex, int size, Vector2 center, float radius)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float px = (x - center.x) / radius;
                float py = (y - center.y) / radius;
                // Heart equation: (x^2 + y^2 - 1)^3 - x^2 * y^3 < 0
                float heart = Mathf.Pow(px * px + py * py - 1, 3) - px * px * py * py * py;
                tex.SetPixel(x, y, heart < 0 ? Color.white : Color.clear);
            }
        }
    }
    
    private void DrawCoin(Texture2D tex, int size, Vector2 center, float radius)
    {
        float innerRadius = radius * 0.6f;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                bool outer = dist < radius;
                bool inner = dist < innerRadius && dist > innerRadius - 2;
                tex.SetPixel(x, y, (outer && !inner) || inner ? Color.white : Color.clear);
            }
        }
    }
    
    private void DrawDiamond(Texture2D tex, int size, Vector2 center, float radius)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = Mathf.Abs(x - center.x);
                float dy = Mathf.Abs(y - center.y);
                // Diamond: |x| + |y| < r
                tex.SetPixel(x, y, dx + dy < radius ? Color.white : Color.clear);
            }
        }
    }
    
    private void DrawPerson(Texture2D tex, int size, Vector2 center, float radius)
    {
        // Clear
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, Color.clear);
        
        // Head (circle at top)
        float headRadius = radius * 0.35f;
        Vector2 headCenter = new Vector2(center.x, center.y + radius * 0.4f);
        
        // Body (triangle at bottom)
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float headDist = Vector2.Distance(new Vector2(x, y), headCenter);
                if (headDist < headRadius)
                {
                    tex.SetPixel(x, y, Color.white);
                    continue;
                }
                
                // Body triangle
                float bodyTop = center.y + radius * 0.1f;
                float bodyBot = center.y - radius;
                if (y < bodyTop && y > bodyBot)
                {
                    float bodyWidth = (bodyTop - y) / (bodyTop - bodyBot) * radius * 0.8f;
                    if (Mathf.Abs(x - center.x) < bodyWidth)
                        tex.SetPixel(x, y, Color.white);
                }
            }
        }
    }
    
    private void DrawCircle(Texture2D tex, int size, Vector2 center, float radius)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist < radius ? Color.white : Color.clear);
            }
        }
    }
    
    private void DrawWrench(Texture2D tex, int size, Vector2 center, float radius)
    {
        // Clear texture
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, Color.clear);
        
        // Draw a simple wrench/spanner shape
        int cx = (int)center.x;
        int cy = (int)center.y;
        int handleWidth = 3;
        int headSize = 6;
        
        // Handle (diagonal line from bottom-left to top-right)
        for (int i = -8; i <= 8; i++)
        {
            for (int w = -handleWidth / 2; w <= handleWidth / 2; w++)
            {
                int px = cx + i + w;
                int py = cy + i;
                if (px >= 0 && px < size && py >= 0 && py < size)
                    tex.SetPixel(px, py, Color.white);
            }
        }
        
        // Wrench head at top-right (open-end style)
        for (int dx = -headSize; dx <= headSize; dx++)
        {
            for (int dy = -headSize; dy <= headSize; dy++)
            {
                int px = cx + 8 + dx;
                int py = cy + 8 + dy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // Ring shape with opening
                bool inRing = dist < headSize && dist > headSize - 3;
                bool inOpening = dx > 0 && Mathf.Abs(dy) < 2;
                if (px >= 0 && px < size && py >= 0 && py < size && inRing && !inOpening)
                    tex.SetPixel(px, py, Color.white);
            }
        }
    }
    
    private void CreateVisuals()
    {
        // Background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = Vector3.zero;
        backgroundSprite = bgObj.AddComponent<SpriteRenderer>();
        backgroundSprite.sprite = CreateSquareSprite();
        backgroundSprite.transform.localScale = new Vector3(roomWidth, roomHeight, 1f);
        backgroundSprite.sortingOrder = 0;
        
        // Highlight border
        var borderObj = new GameObject("HighlightBorder");
        borderObj.transform.SetParent(transform);
        borderObj.transform.localPosition = Vector3.zero;
        highlightBorder = borderObj.AddComponent<SpriteRenderer>();
        highlightBorder.sprite = CreateBorderSprite();
        highlightBorder.transform.localScale = new Vector3(roomWidth + 0.1f, roomHeight + 0.1f, 1f);
        highlightBorder.sortingOrder = 2;
        highlightBorder.enabled = false;
        
        // Progress bar background
        var progressBgObj = new GameObject("ProgressBarBg");
        progressBgObj.transform.SetParent(transform);
        progressBgObj.transform.localPosition = new Vector3(0, -roomHeight / 2 + 0.15f, 0);
        var progressBgSprite = progressBgObj.AddComponent<SpriteRenderer>();
        progressBgSprite.sprite = CreateSquareSprite();
        progressBgSprite.transform.localScale = new Vector3(roomWidth - 0.2f, 0.15f, 1f);
        progressBgSprite.color = new Color(0.2f, 0.2f, 0.2f);
        progressBgSprite.sortingOrder = 1;
        
        // Progress bar fill
        var progressObj = new GameObject("ProgressBar");
        progressObj.transform.SetParent(transform);
        progressObj.transform.localPosition = new Vector3(-(roomWidth - 0.2f) / 2, -roomHeight / 2 + 0.15f, 0);
        progressBar = progressObj.AddComponent<SpriteRenderer>();
        progressBar.sprite = CreateSquareSprite();
        progressBar.color = Color.cyan;
        progressBar.sortingOrder = 2;
        
        // Repair bar (shown above progress bar when room is damaged)
        var repairObj = new GameObject("RepairBar");
        repairObj.transform.SetParent(transform);
        repairObj.transform.localPosition = new Vector3(-(roomWidth - 0.2f) / 2, -roomHeight / 2 + 0.35f, 0);
        repairBar = repairObj.AddComponent<SpriteRenderer>();
        repairBar.sprite = CreateSquareSprite();
        repairBar.color = repairBarColor;
        repairBar.sortingOrder = 2;
        repairBar.enabled = false;
        
        // Label text
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(transform);
        labelObj.transform.localPosition = new Vector3(0, roomHeight / 2 - 0.2f, 0);
        labelText = labelObj.AddComponent<TextMesh>();
        labelText.anchor = TextAnchor.MiddleCenter;
        labelText.alignment = TextAlignment.Center;
        labelText.fontSize = 20;
        labelText.characterSize = 0.1f;
        labelText.color = Color.white;
        
        // Follower slots (square backgrounds) and circles (pawn icons)
        followerSlots = new SpriteRenderer[6];   // Max possible capacity - square slot backgrounds
        followerCircles = new SpriteRenderer[6]; // Circular pawn icons on top of slots
        followerDamageOutlines = new SpriteRenderer[6]; // Damage outlines for each slot
        for (int i = 0; i < followerSlots.Length; i++)
        {
            float xOffset = (i - (followerSlots.Length - 1) / 2f) * (followerIconSize + 0.1f);
            
            // Damage outline (larger square behind everything)
            var outlineObj = new GameObject($"DamageOutline_{i}");
            outlineObj.transform.SetParent(transform);
            outlineObj.transform.localPosition = new Vector3(xOffset, 0, 0);
            followerDamageOutlines[i] = outlineObj.AddComponent<SpriteRenderer>();
            followerDamageOutlines[i].sprite = CreateSquareBorderSprite(); // Square border
            followerDamageOutlines[i].transform.localScale = new Vector3(followerIconSize + 0.08f, followerIconSize + 0.08f, 1f);
            followerDamageOutlines[i].sortingOrder = 2;
            followerDamageOutlines[i].enabled = false;
            
            // Square slot background
            var slotObj = new GameObject($"FollowerSlot_{i}");
            slotObj.transform.SetParent(transform);
            slotObj.transform.localPosition = new Vector3(xOffset, 0, 0);
            followerSlots[i] = slotObj.AddComponent<SpriteRenderer>();
            followerSlots[i].sprite = CreateSquareSprite(); // Square slots
            followerSlots[i].transform.localScale = new Vector3(followerIconSize, followerIconSize, 1f);
            followerSlots[i].sortingOrder = 3;
            followerSlots[i].enabled = false;
            
            // Circular pawn icon (on top of slot)
            var circleObj = new GameObject($"FollowerCircle_{i}");
            circleObj.transform.SetParent(transform);
            circleObj.transform.localPosition = new Vector3(xOffset, 0, 0);
            followerCircles[i] = circleObj.AddComponent<SpriteRenderer>();
            followerCircles[i].sprite = CreateCircleSprite(); // Circular pawns
            followerCircles[i].transform.localScale = new Vector3(followerIconSize * 0.8f, followerIconSize * 0.8f, 1f);
            followerCircles[i].sortingOrder = 4;
            followerCircles[i].enabled = false;
        }
        
        // Level pips (top-right corner)
        CreateLevelPips();
        
        UpdateColor();
    }
    
    private void CreateLevelPips()
    {
        levelPips = new LevelPipVisual[maxLevelDisplay];
        
        float startX = roomWidth / 2 - levelPipSize / 2 - 0.05f;
        float startY = roomHeight / 2 - levelPipSize / 2 - 0.05f;
        
        for (int i = 0; i < maxLevelDisplay; i++)
        {
            var pipObj = new GameObject($"LevelPip_{i}");
            pipObj.transform.SetParent(transform);
            pipObj.transform.localPosition = new Vector3(
                startX - i * (levelPipSize + levelPipSpacing),
                startY,
                0
            );
            
            levelPips[i] = new LevelPipVisual();
            levelPips[i].Create(pipObj.transform, levelPipSize);
        }
    }
    
    private void Update()
    {
        if (room == null) return;
        
        UpdateProgressBar();
        UpdateRepairBar();
        UpdateFollowerIcons();
        UpdateLabel();
        UpdateLevelPips();
    }
    
    private void UpdateColor()
    {
        if (room == null) return;
        
        Color color = room.Type switch
        {
            RoomType.Sanctuary => sanctuaryColor,
            RoomType.Altar => altarColor,
            RoomType.Pews => pewsColor,
            RoomType.Mission => missionColor,
            RoomType.WrathRitualHall => ritualColor,
            RoomType.Workshop => workshopColor,
            _ => emptySlotColor
        };
        
        backgroundSprite.color = color;
    }
    
    private void UpdateProgressBar()
    {
        float progress = room.Progress;
        float maxWidth = roomWidth - 0.2f;
        progressBar.transform.localScale = new Vector3(maxWidth * progress, 0.15f, 1f);
        progressBar.transform.localPosition = new Vector3(
            -(maxWidth / 2) + (maxWidth * progress / 2),
            -roomHeight / 2 + 0.15f,
            0
        );
        
        // Color progress bar based on generated resource type
        progressBar.color = GetResourceColor(room.GeneratedResource);
    }
    
    private Color GetResourceColor(ResourceType resource)
    {
        return resource switch
        {
            ResourceType.Strength => strengthBarColor,
            ResourceType.Favor => favorBarColor,
            ResourceType.Money => moneyBarColor,
            ResourceType.Mask => maskBarColor,
            ResourceType.Blueprint => blueprintBarColor,
            ResourceType.Follower => followerBarColor,
            ResourceType.Repair => repairBarColor,
            _ => defaultBarColor
        };
    }
    
    private void UpdateRepairBar()
    {
        // Only show repair bar when room is damaged
        if (room.Damage > 0)
        {
            repairBar.enabled = true;
            
            float progress = room.RepairProgress;
            float maxWidth = roomWidth - 0.2f;
            repairBar.transform.localScale = new Vector3(maxWidth * progress, 0.1f, 1f);
            repairBar.transform.localPosition = new Vector3(
                -(maxWidth / 2) + (maxWidth * progress / 2),
                -roomHeight / 2 + 0.35f,
                0
            );
        }
        else
        {
            repairBar.enabled = false;
        }
    }
    
    private void UpdateFollowerIcons()
    {
        int level = room.Level;
        int followerCount = room.Followers.Count;
        int orangeDamage = room.OrangeDamage;
        int redDamage = room.RedDamage;
        int capacity = room.Capacity; // Level - RedDamage (slots that can hold pawns)
        int functionalCapacity = room.FunctionalCapacity; // Level - TotalDamage (undamaged slots)
        
        for (int i = 0; i < followerSlots.Length; i++)
        {
            // Calculate slot state from right to left (rightmost slots are damaged first)
            int slotFromRight = level - 1 - i;
            bool isRedSlot = slotFromRight < redDamage;
            bool isOrangeSlot = !isRedSlot && slotFromRight < orangeDamage;
            bool isWithinLevel = i < level;
            
            if (!isWithinLevel)
            {
                // Beyond room level - hide everything
                followerSlots[i].enabled = false;
                followerCircles[i].enabled = false;
                followerDamageOutlines[i].enabled = false;
                continue;
            }
            
            // Show the square slot background
            followerSlots[i].enabled = true;
            
            if (isRedSlot)
            {
                // Red damage slot - show red square, no pawn can be here
                followerSlots[i].color = redDamageColor;
                followerCircles[i].enabled = false;
                followerDamageOutlines[i].enabled = false;
            }
            else if (isOrangeSlot)
            {
                // Orange damage slot - pawn can be here but slot is damaged
                followerSlots[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Dim slot background
                followerDamageOutlines[i].enabled = true;
                followerDamageOutlines[i].color = orangeDamageColor;
                
                // Check if there's a follower in this slot
                int slotIndex = i - redDamage; // Adjust index for followers array (which doesn't include red slots)
                if (slotIndex >= 0 && slotIndex < followerCount)
                {
                    var follower = room.Followers[slotIndex];
                    followerCircles[i].enabled = true;
                    followerCircles[i].color = GetCommitmentColor(follower.Commitment);
                }
                else
                {
                    // Empty orange slot - no pawn circle
                    followerCircles[i].enabled = false;
                }
            }
            else
            {
                // Undamaged slot
                followerSlots[i].color = new Color(0.4f, 0.4f, 0.4f, 0.5f); // Normal slot background
                followerDamageOutlines[i].enabled = false;
                
                // Simpler: pawns fill from left, so just check follower index
                int adjustedIndex = i;
                if (adjustedIndex < followerCount)
                {
                    var follower = room.Followers[adjustedIndex];
                    followerCircles[i].enabled = true;
                    followerCircles[i].color = GetCommitmentColor(follower.Commitment);
                }
                else
                {
                    // Empty slot - no pawn circle
                    followerCircles[i].enabled = false;
                }
            }
        }
    }
    
    private Color GetCommitmentColor(float commitment)
    {
        if (commitment < 25) return Color.red;
        if (commitment < 50) return new Color(1f, 0.5f, 0f); // Orange
        if (commitment < 75) return Color.yellow;
        return Color.green;
    }
    
    private void UpdateLabel()
    {
        string roomName = room.Type.ToString();
        labelText.text = roomName;
        labelText.color = Color.white;
    }
    
    private void UpdateLevelPips()
    {
        int level = room.Level;
        int damage = room.Damage;
        
        for (int i = 0; i < levelPips.Length; i++)
        {
            if (i < maxLevelDisplay)
            {
                bool isFilled = i < level;
                bool isDamaged = i >= (level - damage) && i < level;
                levelPips[i].SetState(isFilled, isDamaged, levelFilledColor, levelEmptyColor, damageOutlineColor);
            }
        }
    }
    
    public void SetHighlight(bool highlighted)
    {
        isHighlighted = highlighted;
        UpdateBorder();
    }
    
    public void SetTargeted(bool targeted)
    {
        isTargeted = targeted;
        UpdateBorder();
    }
    
    private void UpdateBorder()
    {
        if (isTargeted)
        {
            highlightBorder.enabled = true;
            highlightBorder.color = targetColor;
        }
        else if (isHighlighted)
        {
            highlightBorder.enabled = true;
            highlightBorder.color = highlightColor;
        }
        else
        {
            highlightBorder.enabled = false;
        }
    }
    
    // === Sprite Creation Helpers ===
    
    private Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
    
    private Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist < radius ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private Sprite CreateBorderSprite()
    {
        int size = 32;
        int borderWidth = 2;
        Texture2D tex = new Texture2D(size, size);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isBorder = x < borderWidth || x >= size - borderWidth ||
                               y < borderWidth || y >= size - borderWidth;
                tex.SetPixel(x, y, isBorder ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private Sprite CreateSquareBorderSprite()
    {
        int size = 32;
        int borderWidth = 3;
        Texture2D tex = new Texture2D(size, size);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isBorder = x < borderWidth || x >= size - borderWidth ||
                               y < borderWidth || y >= size - borderWidth;
                tex.SetPixel(x, y, isBorder ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Helper class for level pip visualization.
    /// Shows filled/empty circles with optional red damage outline.
    /// </summary>
    private class LevelPipVisual
    {
        private SpriteRenderer fill;
        private SpriteRenderer outline;
        
        public void Create(Transform parent, float size)
        {
            // Outline (slightly larger, behind fill)
            var outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(parent);
            outlineObj.transform.localPosition = Vector3.zero;
            outline = outlineObj.AddComponent<SpriteRenderer>();
            outline.sprite = CreateCircleOutlineSprite();
            outline.transform.localScale = new Vector3(size * 1.3f, size * 1.3f, 1f);
            outline.sortingOrder = 4;
            outline.enabled = false;
            
            // Fill circle
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(parent);
            fillObj.transform.localPosition = Vector3.zero;
            fill = fillObj.AddComponent<SpriteRenderer>();
            fill.sprite = CreateFilledCircleSprite();
            fill.transform.localScale = new Vector3(size, size, 1f);
            fill.sortingOrder = 5;
        }
        
        public void SetState(bool isFilled, bool isDamaged, Color filledColor, Color emptyColor, Color damageColor)
        {
            fill.color = isFilled ? filledColor : emptyColor;
            outline.enabled = isDamaged;
            outline.color = damageColor;
        }
        
        private static Sprite CreateFilledCircleSprite()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, dist < radius - 1 ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
        
        private static Sprite CreateCircleOutlineSprite()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            float radius = size / 2f;
            float innerRadius = radius - 3f;
            Vector2 center = new Vector2(radius, radius);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    bool isRing = dist < radius && dist > innerRadius;
                    tex.SetPixel(x, y, isRing ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
