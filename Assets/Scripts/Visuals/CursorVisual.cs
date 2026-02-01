using UnityEngine;

/// <summary>
/// Visual cursor that shows which room the player currently has selected.
/// </summary>
public class CursorVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController controller;
    
    [Header("Settings")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 0.5f, 0.8f);
    [SerializeField] private Color targetingColor = new Color(1f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color shieldFlashColor = new Color(0.3f, 0.6f, 1f, 1f);
    [SerializeField] private float roomWidth = 1.8f;
    [SerializeField] private float roomHeight = 1.4f;
    [SerializeField] private float roomSpacing = 0.15f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseAmount = 0.1f;
    [SerializeField] private float shieldFlashDuration = 2f;
    
    private SpriteRenderer cursorSprite;
    private SpriteRenderer targetSprite;
    private Transform churchTransform;
    private Transform enemyChurchTransform;
    
    // Shield flash state
    private bool isShieldFlashing = false;
    private float shieldFlashEndTime = 0f;
    private Room currentlySubscribedRoom = null;
    
    private void Awake()
    {
        CreateCursorVisual();
        CreateTargetVisual();
    }
    
    private void Start()
    {
        if (controller != null && controller.Cult?.church != null)
        {
            churchTransform = controller.Cult.church.transform;
        }
        
        // Find enemy church
        var opponent = GameManager.Instance?.GetOpponent(controller?.Cult);
        if (opponent?.church != null)
        {
            enemyChurchTransform = opponent.church.transform;
        }
    }
    
    private void CreateCursorVisual()
    {
        var cursorObj = new GameObject("CursorVisual");
        cursorObj.transform.SetParent(transform);
        cursorSprite = cursorObj.AddComponent<SpriteRenderer>();
        cursorSprite.sprite = CreateBorderSprite();
        cursorSprite.sortingOrder = 100;
        cursorSprite.color = normalColor;
    }
    
    private void CreateTargetVisual()
    {
        var targetObj = new GameObject("TargetVisual");
        targetObj.transform.SetParent(transform);
        targetSprite = targetObj.AddComponent<SpriteRenderer>();
        targetSprite.sprite = CreateCrosshairSprite();
        targetSprite.sortingOrder = 100;
        targetSprite.color = targetingColor;
        targetSprite.enabled = false;
    }
    
    private void Update()
    {
        if (controller == null) return;
        
        UpdateRoomSubscription();
        UpdateShieldFlash();
        UpdateCursorPosition();
        UpdateTargetPosition();
        UpdatePulse();
    }
    
    private void UpdateRoomSubscription()
    {
        // Get the room we're currently hovering over
        Room currentRoom = controller.Cult?.church?.GetRoomAt(controller.CursorPosition);
        
        // If room changed, update subscription
        if (currentRoom != currentlySubscribedRoom)
        {
            // Unsubscribe from old room
            if (currentlySubscribedRoom != null)
            {
                currentlySubscribedRoom.OnDamageBlocked -= OnDamageBlocked;
            }
            
            // Subscribe to new room
            currentlySubscribedRoom = currentRoom;
            if (currentlySubscribedRoom != null)
            {
                currentlySubscribedRoom.OnDamageBlocked += OnDamageBlocked;
            }
        }
    }
    
    private void OnDamageBlocked()
    {
        isShieldFlashing = true;
        shieldFlashEndTime = Time.time + shieldFlashDuration;
    }
    
    private void UpdateShieldFlash()
    {
        if (isShieldFlashing && Time.time >= shieldFlashEndTime)
        {
            isShieldFlashing = false;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from any room we're still subscribed to
        if (currentlySubscribedRoom != null)
        {
            currentlySubscribedRoom.OnDamageBlocked -= OnDamageBlocked;
        }
    }
    
    private void UpdateCursorPosition()
    {
        Vector3 worldPos = GridToWorld(controller.CursorPosition, churchTransform);
        cursorSprite.transform.position = worldPos;
        
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        cursorSprite.transform.localScale = new Vector3(
            (roomWidth + roomSpacing) * scale,
            (roomHeight + roomSpacing) * scale,
            1f
        );
        
        // Determine cursor color and visibility
        if (isShieldFlashing)
        {
            // Flickering blue flash when damage is blocked
            float flicker = Mathf.Sin(Time.time * 20f); // Fast flicker
            cursorSprite.enabled = flicker > 0f; // On/off flicker
            
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) / 2f;
            cursorSprite.color = Color.Lerp(shieldFlashColor * 0.7f, shieldFlashColor, pulse);
        }
        else
        {
            cursorSprite.enabled = true; // Always visible when not flashing
            
            if (controller.IsTargeting)
            {
                cursorSprite.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.4f);
            }
            else
            {
                cursorSprite.color = normalColor;
            }
        }
    }
    
    private void UpdateTargetPosition()
    {
        targetSprite.enabled = controller.IsTargeting;
        
        if (controller.IsTargeting && enemyChurchTransform != null)
        {
            // Calculate world position from target grid position
            Vector3 worldPos = GridToWorld(controller.TargetPosition, enemyChurchTransform);
            targetSprite.transform.position = worldPos;
            
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed * 2f) * pulseAmount * 2f;
            targetSprite.transform.localScale = new Vector3(
                (roomWidth + roomSpacing) * scale,
                (roomHeight + roomSpacing) * scale,
                1f
            );
        }
    }
    
    private void UpdatePulse()
    {
        // Already handled in position updates
    }
    
    private Vector3 GridToWorld(Vector2Int gridPos, Transform reference)
    {
        if (reference == null) return Vector3.zero;
        
        // Get church to calculate grid offset (must match GameInitializer logic)
        var church = reference.GetComponent<Church>();
        int gridW = church != null ? church.GridWidth : 3;
        int gridH = church != null ? church.GridHeight : 4;
        
        float totalWidth = (roomWidth + roomSpacing);
        float totalHeight = (roomHeight + roomSpacing);
        
        // Calculate grid offset so rooms are centered (same as GameInitializer)
        float gridTotalWidth = gridW * totalWidth;
        float gridTotalHeight = gridH * totalHeight;
        Vector3 gridOffset = new Vector3(-gridTotalWidth / 2 + roomWidth / 2, -gridTotalHeight / 2 + roomHeight / 2, 0);
        
        return reference.position + new Vector3(
            gridPos.x * totalWidth,
            gridPos.y * totalHeight,
            0
        ) + gridOffset;
    }
    
    private Sprite CreateBorderSprite()
    {
        int size = 64;
        int borderWidth = 4;
        Texture2D tex = new Texture2D(size, size);
        
        Color clear = Color.clear;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isBorder = x < borderWidth || x >= size - borderWidth ||
                               y < borderWidth || y >= size - borderWidth;
                tex.SetPixel(x, y, isBorder ? Color.white : clear);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private Sprite CreateCrosshairSprite()
    {
        int size = 64;
        int lineWidth = 3;
        int gapSize = 8;
        Texture2D tex = new Texture2D(size, size);
        
        // Clear texture
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, Color.clear);
        
        int center = size / 2;
        
        // Draw crosshair lines with gap in center
        for (int i = 0; i < size; i++)
        {
            // Skip center gap
            if (Mathf.Abs(i - center) < gapSize) continue;
            
            // Horizontal line
            for (int w = -lineWidth / 2; w <= lineWidth / 2; w++)
            {
                if (center + w >= 0 && center + w < size)
                    tex.SetPixel(i, center + w, Color.white);
            }
            
            // Vertical line
            for (int w = -lineWidth / 2; w <= lineWidth / 2; w++)
            {
                if (center + w >= 0 && center + w < size)
                    tex.SetPixel(center + w, i, Color.white);
            }
        }
        
        // Draw corner brackets
        int bracketSize = 10;
        int cornerOffset = 2;
        
        // Top-left
        DrawBracket(tex, cornerOffset, size - cornerOffset - bracketSize, bracketSize, true, true);
        // Top-right
        DrawBracket(tex, size - cornerOffset - bracketSize, size - cornerOffset - bracketSize, bracketSize, false, true);
        // Bottom-left
        DrawBracket(tex, cornerOffset, cornerOffset, bracketSize, true, false);
        // Bottom-right
        DrawBracket(tex, size - cornerOffset - bracketSize, cornerOffset, bracketSize, false, false);
        
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private void DrawBracket(Texture2D tex, int x, int y, int size, bool leftSide, bool topSide)
    {
        int thickness = 2;
        
        // Horizontal part
        for (int i = 0; i < size; i++)
        {
            for (int t = 0; t < thickness; t++)
            {
                int px = leftSide ? x + i : x + size - 1 - i;
                int py = topSide ? y + size - 1 - t : y + t;
                if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    tex.SetPixel(px, py, Color.white);
            }
        }
        
        // Vertical part
        for (int i = 0; i < size; i++)
        {
            for (int t = 0; t < thickness; t++)
            {
                int px = leftSide ? x + t : x + size - 1 - t;
                int py = topSide ? y + i : y + size - 1 - i;
                if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    tex.SetPixel(px, py, Color.white);
            }
        }
    }
    
    public void SetController(PlayerController controller)
    {
        this.controller = controller;
        
        if (controller?.Cult?.church != null)
        {
            churchTransform = controller.Cult.church.transform;
        }
        
        var opponent = GameManager.Instance?.GetOpponent(controller?.Cult);
        if (opponent?.church != null)
        {
            enemyChurchTransform = opponent.church.transform;
        }
    }
}
