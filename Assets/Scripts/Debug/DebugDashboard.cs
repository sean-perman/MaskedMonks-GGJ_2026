using UnityEngine;

/// <summary>
/// Debug visualizer dashboard that displays game state using Unity's IMGUI.
/// Attach to any GameObject in the scene to see the debug overlay.
/// </summary>
public class DebugDashboard : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private bool showDashboard = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F12;
    
    [Header("References")]
    [SerializeField] private Cult cult1;
    [SerializeField] private Cult cult2;
    
    [Header("Layout")]
    [SerializeField] private float panelWidth = 350f;
    [SerializeField] private float panelPadding = 10f;
    
    // Scroll positions
    private Vector2 cult1Scroll;
    private Vector2 cult2Scroll;
    private Vector2 marketplaceScroll;
    
    // Styles
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle valueStyle;
    private GUIStyle warningStyle;
    private GUIStyle criticalStyle;
    private GUIStyle boxStyle;
    private bool stylesInitialized = false;
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showDashboard = !showDashboard;
        }
    }
    
    private void InitStyles()
    {
        if (stylesInitialized) return;
        
        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headerStyle.normal.textColor = Color.white;
        
        subHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };
        subHeaderStyle.normal.textColor = new Color(0.8f, 0.8f, 1f);
        
        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11
        };
        
        warningStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11
        };
        warningStyle.normal.textColor = Color.yellow;
        
        criticalStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11
        };
        criticalStyle.normal.textColor = Color.red;
        
        boxStyle = new GUIStyle(GUI.skin.box);
        
        stylesInitialized = true;
    }
    
    private void OnGUI()
    {
        if (!showDashboard) return;
        
        InitStyles();
        
        // Auto-find cults if not assigned - find by name to ensure correct assignment
        if (cult1 == null || cult2 == null)
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                cult1 = gm.Cult1;
                cult2 = gm.Cult2;
            }
            else
            {
                var cults = FindObjectsByType<Cult>(FindObjectsSortMode.None);
                foreach (var c in cults)
                {
                    if (c.name.Contains("1")) cult1 = c;
                    else if (c.name.Contains("2")) cult2 = c;
                }
            }
        }
        
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // Left panel - Cult 1 (matches left church)
        Rect cult1Rect = new Rect(panelPadding, panelPadding, panelWidth, screenHeight - panelPadding * 2);
        DrawCultPanel(cult1Rect, cult1, "CULT 1", ref cult1Scroll, new Color(0.2f, 0.3f, 0.5f, 0.9f));
        
        // Right panel - Cult 2 (matches right church)
        Rect cult2Rect = new Rect(screenWidth - panelWidth - panelPadding, panelPadding, panelWidth, screenHeight - panelPadding * 2);
        DrawCultPanel(cult2Rect, cult2, "CULT 2", ref cult2Scroll, new Color(0.5f, 0.2f, 0.3f, 0.9f));
        
        // Center bottom - Marketplace & Game State
        float centerWidth = 300f;
        float centerHeight = 200f;
        Rect centerRect = new Rect((screenWidth - centerWidth) / 2, screenHeight - centerHeight - panelPadding, centerWidth, centerHeight);
        DrawMarketplacePanel(centerRect);
        
        // Top center - Game Time
        Rect gameTimeRect = new Rect((screenWidth - 200) / 2, panelPadding, 200, 50);
        DrawGameTimePanel(gameTimeRect);
        
        // Toggle hint
        GUI.Label(new Rect(screenWidth - 150, screenHeight - 25, 140, 20), $"Press {toggleKey} to toggle", valueStyle);
    }
    
    private void DrawCultPanel(Rect rect, Cult cult, string title, ref Vector2 scrollPos, Color bgColor)
    {
        // Background
        GUI.color = bgColor;
        GUI.Box(rect, "", boxStyle);
        GUI.color = Color.white;
        
        GUILayout.BeginArea(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));
        
        GUILayout.Label(title, headerStyle);
        GUILayout.Space(5);
        
        if (cult == null)
        {
            GUILayout.Label("No cult assigned", warningStyle);
            GUILayout.EndArea();
            return;
        }
        
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        
        // God Section
        DrawGodSection(cult.god);
        
        GUILayout.Space(10);
        
        // Church/Rooms Section
        DrawChurchSection(cult.church);
        
        GUILayout.Space(10);
        
        // Followers Section
        DrawFollowersSection(cult);
        
        GUILayout.Space(10);
        
        // Resources Section
        DrawResourcesSection(cult);
        
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    private void DrawGodSection(God god)
    {
        GUILayout.Label("═══ GOD ═══", subHeaderStyle);
        
        if (god == null)
        {
            GUILayout.Label("  No god!", criticalStyle);
            return;
        }
        
        // Strength bar
        float strengthPercent = god.MaxStrength > 0 ? (float)god.Strength / god.MaxStrength : 0;
        DrawStatBar("Strength", god.Strength, god.MaxStrength, strengthPercent, Color.red);
        
        // Favor bar
        float favorPercent = god.MaxFavor > 0 ? (float)god.Favor / god.MaxFavor : 0;
        DrawStatBar("Favor", god.Favor, god.MaxFavor, favorPercent, Color.yellow);
        
        // Masks
        GUILayout.Label($"  Masks in Storage: {god.StoredMasks.Count}/4", valueStyle);
        
        if (god.StoredMasks.Count > 0)
        {
            foreach (var mask in god.StoredMasks)
            {
                string shelfLife = mask.ShelfLife > 0 ? $" ({mask.ShelfLife:F0}s)" : " (EXPIRED)";
                var style = mask.ShelfLife < 10 ? warningStyle : valueStyle;
                GUILayout.Label($"    • {mask.Type}{shelfLife}", style);
            }
        }
        
        // Current mask
        if (god.CurrentMask != null)
        {
            GUILayout.Label($"  Wearing: {god.CurrentMask.Type}", valueStyle);
        }
        
        // Over-time effects
        // Using reflection or we need public properties - for now just show if active
    }
    
    private void DrawChurchSection(Church church)
    {
        GUILayout.Label("═══ CHURCH ═══", subHeaderStyle);
        
        if (church == null)
        {
            GUILayout.Label("  No church!", criticalStyle);
            return;
        }
        
        GUILayout.Label($"  Grid: {church.GridWidth}x{church.GridHeight}", valueStyle);
        GUILayout.Label($"  Rooms: {church.Rooms.Count}", valueStyle);
        
        foreach (var room in church.Rooms)
        {
            if (room == null) continue;
            
            string roomInfo = $"  [{room.Location.x},{room.Location.y}] {room.Type}";
            GUILayout.Label(roomInfo, subHeaderStyle);
            
            // Level and damage
            string levelDamage = $"    Lvl {room.Level}";
            if (room.Damage > 0)
            {
                levelDamage += $" | DMG: {room.Damage}";
            }
            GUILayout.Label(levelDamage, room.Damage > 0 ? warningStyle : valueStyle);
            
            // Capacity
            GUILayout.Label($"    Followers: {room.Followers.Count}/{room.Capacity}", valueStyle);
            
            // Progress bar
            if (room.Followers.Count > 0)
            {
                DrawProgressBar("    Progress", room.Progress, Color.cyan);
            }
        }
    }
    
    private void DrawFollowersSection(Cult cult)
    {
        GUILayout.Label("═══ FOLLOWERS ═══", subHeaderStyle);
        
        int totalFollowers = cult.FollowerCount;
        var style = totalFollowers <= 2 ? criticalStyle : (totalFollowers <= 5 ? warningStyle : valueStyle);
        GUILayout.Label($"  Total: {totalFollowers}", style);
        
        foreach (var follower in cult.Followers)
        {
            if (follower == null) continue;
            
            string location = follower.CurrentRoom != null ? follower.CurrentRoom.Type.ToString() : "Unassigned";
            float commitment = follower.Commitment;
            
            var commitStyle = commitment < 25 ? criticalStyle : (commitment < 50 ? warningStyle : valueStyle);
            GUILayout.Label($"  • {follower.name}: {commitment:F0}% @ {location}", commitStyle);
        }
    }
    
    private void DrawResourcesSection(Cult cult)
    {
        GUILayout.Label("═══ RESOURCES ═══", subHeaderStyle);
        GUILayout.Label($"  Money: ${cult.Money:F0}", valueStyle);
    }
    
    private void DrawMarketplacePanel(Rect rect)
    {
        GUI.color = new Color(0.3f, 0.4f, 0.3f, 0.9f);
        GUI.Box(rect, "", boxStyle);
        GUI.color = Color.white;
        
        GUILayout.BeginArea(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));
        
        GUILayout.Label("MARKETPLACE", headerStyle);
        
        var marketplace = Marketplace.Instance;
        if (marketplace == null)
        {
            GUILayout.Label("No marketplace found", warningStyle);
            GUILayout.EndArea();
            return;
        }
        
        GUILayout.Label($"Citizens: {marketplace.CitizenCount}/10", valueStyle);
        GUILayout.Label($"Full: {marketplace.IsFull}", valueStyle);
        
        marketplaceScroll = GUILayout.BeginScrollView(marketplaceScroll, GUILayout.Height(100));
        foreach (var citizen in marketplace.Citizens)
        {
            if (citizen != null)
            {
                GUILayout.Label($"  • {citizen.name}", valueStyle);
            }
        }
        GUILayout.EndScrollView();
        
        GUILayout.EndArea();
    }
    
    private void DrawGameTimePanel(Rect rect)
    {
        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        GUI.Box(rect, "", boxStyle);
        GUI.color = Color.white;
        
        GUILayout.BeginArea(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));
        
        var gm = GameManager.Instance;
        if (gm != null)
        {
            string status = gm.IsGameRunning ? "RUNNING" : "STOPPED";
            int minutes = Mathf.FloorToInt(gm.GameTime / 60);
            int seconds = Mathf.FloorToInt(gm.GameTime % 60);
            GUILayout.Label($"Game: {status} | {minutes:00}:{seconds:00}", headerStyle);
        }
        else
        {
            GUILayout.Label("No GameManager", warningStyle);
        }
        
        GUILayout.EndArea();
    }
    
    private void DrawStatBar(string label, int current, int max, float percent, Color barColor)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"  {label}: {current}/{max}", valueStyle, GUILayout.Width(120));
        
        // Draw bar background
        Rect barRect = GUILayoutUtility.GetRect(150, 16);
        GUI.color = Color.gray;
        GUI.Box(barRect, "");
        
        // Draw filled portion
        GUI.color = barColor;
        Rect filledRect = new Rect(barRect.x, barRect.y, barRect.width * percent, barRect.height);
        GUI.DrawTexture(filledRect, Texture2D.whiteTexture);
        
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
    }
    
    private void DrawProgressBar(string label, float percent, Color barColor)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, valueStyle, GUILayout.Width(80));
        
        Rect barRect = GUILayoutUtility.GetRect(100, 12);
        GUI.color = Color.gray;
        GUI.Box(barRect, "");
        
        GUI.color = barColor;
        Rect filledRect = new Rect(barRect.x, barRect.y, barRect.width * percent, barRect.height);
        GUI.DrawTexture(filledRect, Texture2D.whiteTexture);
        
        GUI.color = Color.white;
        GUILayout.Label($"{percent:P0}", valueStyle, GUILayout.Width(50));
        GUILayout.EndHorizontal();
    }
}
