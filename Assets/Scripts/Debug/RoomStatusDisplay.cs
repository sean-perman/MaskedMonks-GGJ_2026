using UnityEngine;

/// <summary>
/// Displays room status as a floating label in world space.
/// Attach to each room GameObject for visual debugging.
/// </summary>
public class RoomStatusDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Room room;
    
    [Header("Display Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);
    [SerializeField] private bool autoFindRoom = true;
    
    private GUIStyle labelStyle;
    private GUIStyle progressStyle;
    private bool stylesInit = false;
    
    private void Start()
    {
        if (autoFindRoom && room == null)
        {
            room = GetComponent<Room>();
        }
    }
    
    private void InitStyles()
    {
        if (stylesInit) return;
        
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };
        labelStyle.normal.textColor = Color.white;
        labelStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.6f));
        labelStyle.padding = new RectOffset(4, 4, 2, 2);
        
        progressStyle = new GUIStyle(labelStyle);
        progressStyle.normal.textColor = Color.cyan;
        
        stylesInit = true;
    }
    
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
    
    private void OnGUI()
    {
        if (room == null || Camera.main == null) return;
        
        InitStyles();
        
        Vector3 worldPos = transform.position + offset;
        Vector3 screenPos3D = Camera.main.WorldToScreenPoint(worldPos);
        
        // Behind camera check
        if (screenPos3D.z < 0) return;
        
        Vector2 screenPos = new Vector2(screenPos3D.x, Screen.height - screenPos3D.y);
        
        // Build display text
        string roomName = room.Type.ToString();
        string level = $"Lv{room.Level}";
        string damage = room.Damage > 0 ? $" DMG:{room.Damage}" : "";
        string followers = $"{room.Followers.Count}/{room.Capacity}";
        
        string line1 = $"{roomName} {level}{damage}";
        string line2 = $"Workers: {followers}";
        
        // Calculate size
        float width = 120f;
        float height = 50f;
        
        Rect bgRect = new Rect(screenPos.x - width / 2, screenPos.y - height / 2, width, height);
        
        // Background
        GUI.color = GetRoomColor();
        GUI.Box(bgRect, "", GUI.skin.box);
        GUI.color = Color.white;
        
        // Labels
        GUI.Label(new Rect(bgRect.x, bgRect.y + 2, bgRect.width, 18), line1, labelStyle);
        GUI.Label(new Rect(bgRect.x, bgRect.y + 18, bgRect.width, 14), line2, labelStyle);
        
        // Progress bar
        if (room.Followers.Count > 0)
        {
            Rect barBg = new Rect(bgRect.x + 5, bgRect.y + 34, bgRect.width - 10, 8);
            GUI.color = Color.gray;
            GUI.DrawTexture(barBg, Texture2D.whiteTexture);
            
            GUI.color = Color.cyan;
            Rect barFill = new Rect(barBg.x, barBg.y, barBg.width * room.Progress, barBg.height);
            GUI.DrawTexture(barFill, Texture2D.whiteTexture);
            
            GUI.color = Color.white;
        }
    }
    
    private Color GetRoomColor()
    {
        if (room == null) return new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        return room.Type switch
        {
            RoomType.Sanctuary => new Color(0.2f, 0.5f, 0.2f, 0.8f),
            RoomType.Altar => new Color(0.5f, 0.2f, 0.2f, 0.8f),
            RoomType.Pews => new Color(0.5f, 0.5f, 0.2f, 0.8f),
            RoomType.Mission => new Color(0.2f, 0.4f, 0.5f, 0.8f),
            RoomType.WrathRitualHall => new Color(0.4f, 0.2f, 0.5f, 0.8f),
            RoomType.Workshop => new Color(0.5f, 0.4f, 0.2f, 0.8f),
            _ => new Color(0.3f, 0.3f, 0.3f, 0.8f)
        };
    }
    
    /// <summary>
    /// Set the room to display.
    /// </summary>
    public void SetRoom(Room newRoom)
    {
        room = newRoom;
    }
}
