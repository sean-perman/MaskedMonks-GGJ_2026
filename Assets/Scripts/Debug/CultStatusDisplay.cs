using UnityEngine;

/// <summary>
/// A simplified single-cult visualizer that can be positioned in world space.
/// Good for showing status above each cult's church.
/// </summary>
public class CultStatusDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Cult cult;
    
    [Header("Display Settings")]
    [SerializeField] private bool showInWorldSpace = true;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float displayWidth = 200f;
    [SerializeField] private float displayHeight = 150f;
    
    private GUIStyle headerStyle;
    private GUIStyle valueStyle;
    private GUIStyle warningStyle;
    private GUIStyle criticalStyle;
    private bool stylesInit = false;
    
    private void InitStyles()
    {
        if (stylesInit) return;
        
        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headerStyle.normal.textColor = Color.white;
        
        valueStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
        valueStyle.normal.textColor = Color.white;
        
        warningStyle = new GUIStyle(valueStyle);
        warningStyle.normal.textColor = Color.yellow;
        
        criticalStyle = new GUIStyle(valueStyle);
        criticalStyle.normal.textColor = Color.red;
        
        stylesInit = true;
    }
    
    private void OnGUI()
    {
        if (cult == null) return;
        
        InitStyles();
        
        Vector2 screenPos;
        
        if (showInWorldSpace && Camera.main != null)
        {
            Vector3 worldPos = transform.position + worldOffset;
            screenPos = Camera.main.WorldToScreenPoint(worldPos);
            screenPos.y = Screen.height - screenPos.y; // Flip Y for GUI
            
            // Don't draw if behind camera
            if (Camera.main.WorldToScreenPoint(worldPos).z < 0) return;
        }
        else
        {
            screenPos = new Vector2(10, 10);
        }
        
        Rect displayRect = new Rect(screenPos.x - displayWidth / 2, screenPos.y, displayWidth, displayHeight);
        
        // Background
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(displayRect, "");
        GUI.color = Color.white;
        
        GUILayout.BeginArea(displayRect);
        
        GUILayout.Label(cult.name, headerStyle);
        
        // God stats
        if (cult.god != null)
        {
            var god = cult.god;
            
            // Strength
            float strPct = god.MaxStrength > 0 ? (float)god.Strength / god.MaxStrength : 0;
            var strStyle = strPct < 0.25f ? criticalStyle : (strPct < 0.5f ? warningStyle : valueStyle);
            DrawMiniBar("STR", god.Strength, god.MaxStrength, strPct, Color.red, strStyle);
            
            // Favor
            float favPct = god.MaxFavor > 0 ? (float)god.Favor / god.MaxFavor : 0;
            var favStyle = favPct < 0.25f ? criticalStyle : (favPct < 0.5f ? warningStyle : valueStyle);
            DrawMiniBar("FAV", god.Favor, god.MaxFavor, favPct, Color.yellow, favStyle);
            
            // Masks
            GUILayout.Label($"Masks: {god.StoredMasks.Count}/4", valueStyle);
        }
        
        // Followers
        var folStyle = cult.FollowerCount <= 2 ? criticalStyle : (cult.FollowerCount <= 5 ? warningStyle : valueStyle);
        GUILayout.Label($"Followers: {cult.FollowerCount}", folStyle);
        
        // Money
        GUILayout.Label($"Money: ${cult.Money:F0}", valueStyle);
        
        GUILayout.EndArea();
    }
    
    private void DrawMiniBar(string label, int current, int max, float percent, Color barColor, GUIStyle textStyle)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {current}/{max}", textStyle, GUILayout.Width(80));
        
        Rect barRect = GUILayoutUtility.GetRect(100, 14);
        
        // Background
        GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        GUI.DrawTexture(barRect, Texture2D.whiteTexture);
        
        // Filled
        GUI.color = barColor;
        Rect filled = new Rect(barRect.x + 1, barRect.y + 1, (barRect.width - 2) * percent, barRect.height - 2);
        GUI.DrawTexture(filled, Texture2D.whiteTexture);
        
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// Set the cult to display.
    /// </summary>
    public void SetCult(Cult newCult)
    {
        cult = newCult;
    }
}
