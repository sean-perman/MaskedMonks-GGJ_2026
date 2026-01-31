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
    [SerializeField] private Color repairBarColor = new Color(0.3f, 0.8f, 0.3f);
    
    private Room room;
    private SpriteRenderer backgroundSprite;
    private SpriteRenderer progressBar;
    private SpriteRenderer repairBar;
    private SpriteRenderer[] followerIcons;
    private SpriteRenderer highlightBorder;
    private TextMesh labelText;
    private LevelPipVisual[] levelPips;
    
    private bool isHighlighted = false;
    private bool isTargeted = false;
    
    private void Awake()
    {
        room = GetComponent<Room>();
        CreateVisuals();
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
        
        // Follower icons (create max capacity icons, hide unused)
        followerIcons = new SpriteRenderer[6]; // Max possible capacity
        for (int i = 0; i < followerIcons.Length; i++)
        {
            var iconObj = new GameObject($"FollowerIcon_{i}");
            iconObj.transform.SetParent(transform);
            float xOffset = (i - (followerIcons.Length - 1) / 2f) * (followerIconSize + 0.1f);
            iconObj.transform.localPosition = new Vector3(xOffset, 0, 0);
            followerIcons[i] = iconObj.AddComponent<SpriteRenderer>();
            followerIcons[i].sprite = CreateCircleSprite();
            followerIcons[i].transform.localScale = new Vector3(followerIconSize, followerIconSize, 1f);
            followerIcons[i].sortingOrder = 3;
            followerIcons[i].enabled = false;
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
            RoomType.RitualHall => ritualColor,
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
        int capacity = room.Capacity;
        int followerCount = room.Followers.Count;
        
        for (int i = 0; i < followerIcons.Length; i++)
        {
            if (i < capacity)
            {
                followerIcons[i].enabled = true;
                if (i < followerCount)
                {
                    // Filled slot - show follower commitment color
                    var follower = room.Followers[i];
                    followerIcons[i].color = GetCommitmentColor(follower.Commitment);
                }
                else
                {
                    // Empty slot
                    followerIcons[i].color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                }
            }
            else
            {
                followerIcons[i].enabled = false;
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
