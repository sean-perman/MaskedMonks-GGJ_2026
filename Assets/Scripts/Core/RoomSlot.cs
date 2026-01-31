using UnityEngine;

/// <summary>
/// Represents an empty slot in the church grid where a room can be built.
/// </summary>
public class RoomSlot : MonoBehaviour
{
    [SerializeField] private Vector2Int location;
    [SerializeField] private bool isBuilt = false;
    
    private Church church;
    private Room builtRoom;
    
    // Visual components
    private SpriteRenderer spriteRenderer;
    
    public Vector2Int Location => location;
    public bool IsBuilt => isBuilt;
    public Room BuiltRoom => builtRoom;
    
    public void Initialize(Church church, Vector2Int location)
    {
        this.church = church;
        this.location = location;
        this.isBuilt = false;
        
        // Create visual representation
        SetupVisuals();
    }
    
    private void SetupVisuals()
    {
        // Add sprite renderer if not present
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Create a simple quad texture for empty slot
        spriteRenderer.sprite = CreateSlotSprite();
        spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        spriteRenderer.sortingOrder = 0;
    }
    
    private Sprite CreateSlotSprite()
    {
        // Create a simple 1x1 white texture
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
    
    /// <summary>
    /// Build a room in this slot.
    /// </summary>
    public bool BuildRoom(Room roomPrefab)
    {
        if (isBuilt) return false;
        
        builtRoom = Instantiate(roomPrefab, transform);
        builtRoom.transform.localPosition = Vector3.zero;
        isBuilt = true;
        
        // Hide slot visual
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Demolish the room in this slot.
    /// </summary>
    public void DemolishRoom()
    {
        if (!isBuilt || builtRoom == null) return;
        
        Destroy(builtRoom.gameObject);
        builtRoom = null;
        isBuilt = false;
        
        // Show slot visual
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }
    
    public void SetHighlight(bool highlighted, Color highlightColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlighted ? highlightColor : new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
    }
}
