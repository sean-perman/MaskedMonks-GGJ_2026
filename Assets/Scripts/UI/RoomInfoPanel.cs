using UnityEngine;

/// <summary>
/// Side panel that displays detailed information about the currently selected room.
/// Shows room name, what it generates, timing, level, upgrade cost, and key bindings.
/// </summary>
public class RoomInfoPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController controller;
    
    [Header("Panel Settings")]
    [SerializeField] private bool isLeftSide = true; // Left for P1, Right for P2
    [SerializeField] private float panelWidth = 200f;
    [SerializeField] private float panelPadding = 5f;
    [SerializeField] private float screenEdgeMargin = 5f; // Distance from screen edge
    
    [Header("Colors")]
    [SerializeField] private Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
    [SerializeField] private Color headerColor = new Color(1f, 0.9f, 0.5f);
    [SerializeField] private Color labelColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color valueColor = Color.white;
    [SerializeField] private Color keyColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] private Color canAffordColor = new Color(0.4f, 1f, 0.4f);
    [SerializeField] private Color cannotAffordColor = new Color(1f, 0.4f, 0.4f);
    
    // Styles
    private GUIStyle panelStyle;
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private GUIStyle keyStyle;
    private GUIStyle descriptionStyle;
    private Texture2D panelTexture;
    private bool stylesInit = false;
    
    public void SetController(PlayerController controller, bool leftSide)
    {
        this.controller = controller;
        this.isLeftSide = leftSide;
    }
    
    private void InitStyles()
    {
        if (stylesInit) return;
        
        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, panelBackgroundColor);
        panelTexture.Apply();
        
        panelStyle = new GUIStyle();
        panelStyle.normal.background = panelTexture;
        panelStyle.padding = new RectOffset((int)panelPadding, (int)panelPadding, (int)panelPadding, (int)panelPadding);
        
        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headerStyle.normal.textColor = headerColor;
        
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12
        };
        labelStyle.normal.textColor = labelColor;
        
        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        valueStyle.normal.textColor = valueColor;
        
        keyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };
        keyStyle.normal.textColor = keyColor;
        
        descriptionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            wordWrap = true
        };
        descriptionStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        
        stylesInit = true;
    }
    
    private void OnGUI()
    {
        if (controller == null || controller.Cult == null) return;
        
        InitStyles();
        
        // Get current room
        Room currentRoom = controller.Cult.church?.GetRoomAt(controller.CursorPosition);
        if (currentRoom == null) return;
        
        // Don't show panel when targeting
        if (controller.IsTargeting) return;
        
        // Calculate panel position - anchor to outer edges of screen
        float panelHeight = IsMaskGeneratingRoom(currentRoom.Type) ? 420f : 320f;
        float x = isLeftSide ? screenEdgeMargin : Screen.width - panelWidth - screenEdgeMargin;
        float y = (Screen.height - panelHeight) / 2f;
        
        Rect panelRect = new Rect(x, y, panelWidth, panelHeight);
        
        // Draw panel
        GUILayout.BeginArea(panelRect, panelStyle);
        
        // Room name header
        GUILayout.Label(GetRoomDisplayName(currentRoom.Type), headerStyle);
        GUILayout.Space(8);
        
        // Divider
        DrawDivider();
        GUILayout.Space(5);
        
        // Description
        GUILayout.Label(GetRoomDescription(currentRoom.Type), descriptionStyle);
        GUILayout.Space(8);
        
        // Stats section
        DrawStatRow("Generates:", GetGeneratesText(currentRoom));
        DrawStatRow("Cycle Time:", $"{currentRoom.Duration:F0} pawn-seconds");
        DrawStatRow("Progress:", $"{currentRoom.Progress * 100:F0}%");
        
        // Show mask details for attack rooms
        if (IsMaskGeneratingRoom(currentRoom.Type))
        {
            GUILayout.Space(5);
            DrawDivider();
            GUILayout.Space(5);
            DrawMaskDetails(currentRoom.Type);
        }
        
        GUILayout.Space(5);
        DrawDivider();
        GUILayout.Space(5);
        
        // Level info
        if (currentRoom.IsBuilt)
        {
            DrawStatRow("Level:", $"{currentRoom.Level}");
            DrawStatRow("Capacity:", $"{currentRoom.Followers.Count} / {currentRoom.Capacity}");
        }
        else
        {
            DrawStatRow("Status:", "Not Built");
        }

        // Upgrade/Build cost
        int upgradeCost = currentRoom.UpgradeCost;
        int playerMoney = controller.Cult.Money;
        bool canAfford = playerMoney >= upgradeCost;
        string costLabel = currentRoom.IsBuilt ? "Upgrade Cost:" : "Build Cost:";

        GUILayout.BeginHorizontal();
        GUILayout.Label(costLabel, labelStyle, GUILayout.Width(90));
        var costStyle = new GUIStyle(valueStyle);
        costStyle.normal.textColor = canAfford ? canAffordColor : cannotAffordColor;
        GUILayout.Label($"{upgradeCost} gold", costStyle);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        DrawDivider();
        GUILayout.Space(5);

        // Controls section
        GUILayout.Label("Controls", headerStyle);
        GUILayout.Space(5);

        var bindings = controller.Bindings;
        if (currentRoom.IsBuilt)
        {
            DrawKeyBinding("Add Follower:", bindings.sendFromSanctuary);
            DrawKeyBinding("Remove Follower:", bindings.sendToSanctuary);
            DrawKeyBinding("Upgrade Room:", bindings.upgradeRoom);
        }
        else
        {
            DrawKeyBinding("Build Room:", bindings.upgradeRoom);
        }
        
        GUILayout.EndArea();
    }
    
    private void DrawStatRow(string label, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, labelStyle, GUILayout.Width(90));
        GUILayout.Label(value, valueStyle);
        GUILayout.EndHorizontal();
    }
    
    private void DrawKeyBinding(string action, KeyCode key)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(action, labelStyle, GUILayout.Width(110));
        GUILayout.Label($"[{FormatKeyName(key)}]", keyStyle);
        GUILayout.EndHorizontal();
    }
    
    private void DrawDivider()
    {
        var dividerRect = GUILayoutUtility.GetRect(panelWidth - panelPadding * 2, 1);
        GUI.DrawTexture(dividerRect, Texture2D.whiteTexture);
    }
    
    private string FormatKeyName(KeyCode key)
    {
        return key switch
        {
            KeyCode.Alpha1 => "1",
            KeyCode.Alpha2 => "2",
            KeyCode.Alpha3 => "3",
            KeyCode.Alpha4 => "4",
            KeyCode.BackQuote => "`",
            KeyCode.LeftControl => "LCtrl",
            KeyCode.RightControl => "RCtrl",
            KeyCode.Comma => ",",
            KeyCode.Period => ".",
            KeyCode.Slash => "/",
            KeyCode.Semicolon => ";",
            KeyCode.Quote => "'",
            KeyCode.Return => "Enter",
            _ => key.ToString()
        };
    }
    
    private string GetRoomDisplayName(RoomType type)
    {
        return type switch
        {
            RoomType.Sanctuary => "Sanctuary",
            RoomType.Altar => "Altar",
            RoomType.Pews => "Pews",
            RoomType.Mission => "Mission House",
            RoomType.WrathRitualHall => "Wrath Ritual Hall",
            RoomType.Workshop => "Workshop",
            RoomType.Fundraising => "Fundraising Hall",
            RoomType.LightningRitual => "Lightning Ritual",
            RoomType.FloodRitual => "Flood Ritual",
            RoomType.ShieldRitual => "Shield Ritual",
            RoomType.Empty => "Empty Slot",
            _ => type.ToString()
        };
    }
    
    private string GetRoomDescription(RoomType type)
    {
        return type switch
        {
            RoomType.Sanctuary => "A safe haven where followers rest and recover their commitment. No work is done here.",
            RoomType.Altar => "Followers worship here to increase your God's strength through prayer and sacrifice.",
            RoomType.Pews => "Followers pray here to generate divine favor. Commitment does not decay.",
            RoomType.Mission => "Sends missionaries to recruit new followers from the marketplace.",
            RoomType.WrathRitualHall => "Crafts Strike masks that deal damage to enemy rooms.",
            RoomType.Workshop => "Repairs all damaged rooms in your church when the cycle completes.",
            RoomType.Fundraising => "Collects donations to generate gold, but costs favor to operate.",
            RoomType.LightningRitual => "Creates Lightning masks that strike an entire column of enemy rooms.",
            RoomType.FloodRitual => "Creates Flood masks that devastate the bottom row of enemy rooms.",
            RoomType.ShieldRitual => "Creates Shield masks that automatically block incoming attacks.",
            RoomType.Empty => "An empty building slot.",
            _ => "A room in your church."
        };
    }
    
    private string GetGeneratesText(Room room)
    {
        return room.GeneratedResource switch
        {
            ResourceType.Strength => "God Strength",
            ResourceType.Favor => "Divine Favor",
            ResourceType.Money => "Gold",
            ResourceType.Mask => "Attack Masks",
            ResourceType.Follower => "New Followers",
            ResourceType.Repair => "Room Repairs",
            ResourceType.Blueprint => "Blueprints",
            ResourceType.None => "Rest & Recovery",
            _ => "Unknown"
        };
    }
    
    private bool IsMaskGeneratingRoom(RoomType type)
    {
        return type == RoomType.WrathRitualHall ||
               type == RoomType.LightningRitual ||
               type == RoomType.FloodRitual ||
               type == RoomType.ShieldRitual;
    }
    
    private void DrawMaskDetails(RoomType type)
    {
        var config = GameConfig.Instance;
        
        GUILayout.Label("Mask Details", headerStyle);
        GUILayout.Space(3);
        
        switch (type)
        {
            case RoomType.WrathRitualHall:
                DrawStatRow("Favor Cost:", $"{config.ritualHallMaskFavorCost}");
                DrawStatRow("Damage:", $"{config.ritualHallMaskEffectValue} to target");
                DrawStatRow("Shelf Life:", $"{config.ritualHallMaskShelfLife:F0}s");
                break;
                
            case RoomType.LightningRitual:
                DrawStatRow("Favor Cost:", $"{config.lightningMaskFavorCost}");
                DrawStatRow("Damage:", $"{config.lightningDamagePerRoom} per room");
                DrawStatRow("Effect:", "Hits entire column");
                DrawStatRow("Shelf Life:", $"{config.lightningMaskShelfLife:F0}s");
                break;
                
            case RoomType.FloodRitual:
                DrawStatRow("Favor Cost:", $"{config.floodMaskFavorCost}");
                DrawStatRow("Damage:", $"{config.floodDamagePerRoom} per room");
                DrawStatRow("Effect:", "Hits bottom row");
                DrawStatRow("Shelf Life:", $"{config.floodMaskShelfLife:F0}s");
                break;
                
            case RoomType.ShieldRitual:
                DrawStatRow("Favor Cost:", $"{config.shieldFavorCost}");
                DrawStatRow("Effect:", "Auto-blocks attack");
                DrawStatRow("Shelf Life:", $"{config.shieldMaskShelfLife:F0}s");
                break;
        }
    }
}
