using UnityEngine;

/// <summary>
/// In-game menu for viewing and rebinding controls for both players.
/// Toggle with Escape key.
/// </summary>
public class ControlsMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private bool isOpen = false;
    
    [Header("References")]
    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;
    
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle bindingStyle;
    private bool stylesInit = false;
    
    private bool isRebinding = false;
    private int rebindingPlayer = -1;
    private string rebindingAction = null;
    
    private Vector2 scrollPosition;
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && !isRebinding)
        {
            isOpen = !isOpen;
            
            // Pause game when menu is open
            Time.timeScale = isOpen ? 0f : 1f;
        }
        
        // Handle rebinding
        if (isRebinding)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key) && key != KeyCode.Escape && key != KeyCode.Mouse0)
                {
                    ApplyRebinding(key);
                    break;
                }
            }
            
            // Cancel with escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelRebinding();
            }
        }
    }
    
    private void InitStyles()
    {
        if (stylesInit) return;
        
        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headerStyle.normal.textColor = Color.white;
        
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
        labelStyle.normal.textColor = Color.white;
        
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 12 };
        
        bindingStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };
        bindingStyle.normal.textColor = Color.cyan;
        
        stylesInit = true;
    }
    
    private void OnGUI()
    {
        if (!isOpen) return;
        
        InitStyles();
        
        // Darken background
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        
        // Menu window
        float menuWidth = 800f;
        float menuHeight = 600f;
        Rect menuRect = new Rect(
            (Screen.width - menuWidth) / 2,
            (Screen.height - menuHeight) / 2,
            menuWidth,
            menuHeight
        );
        
        GUI.Box(menuRect, "");
        
        GUILayout.BeginArea(new Rect(menuRect.x + 20, menuRect.y + 20, menuRect.width - 40, menuRect.height - 40));
        
        // Title
        GUILayout.Label("CONTROLS MENU", headerStyle);
        GUILayout.Space(10);
        
        if (isRebinding)
        {
            GUILayout.Label($"Press a key to bind to {rebindingAction} for Player {rebindingPlayer + 1}", labelStyle);
            GUILayout.Label("Press Escape to cancel", labelStyle);
        }
        else
        {
            // Scroll view for controls
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.BeginHorizontal();
            
            // Player 1 controls
            GUILayout.BeginVertical(GUILayout.Width(360));
            DrawPlayerControls(0, player1);
            GUILayout.EndVertical();
            
            GUILayout.Space(20);
            
            // Player 2 controls
            GUILayout.BeginVertical(GUILayout.Width(360));
            DrawPlayerControls(1, player2);
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndScrollView();
            
            GUILayout.Space(10);
            
            // Buttons
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset P1 to Defaults", GUILayout.Height(30)))
            {
                ResetToDefaults(0);
            }
            
            if (GUILayout.Button("Reset P2 to Defaults", GUILayout.Height(30)))
            {
                ResetToDefaults(1);
            }
            
            if (GUILayout.Button("Close (Escape)", GUILayout.Height(30)))
            {
                isOpen = false;
                Time.timeScale = 1f;
            }
            
            GUILayout.EndHorizontal();
        }
        
        GUILayout.EndArea();
    }
    
    private void DrawPlayerControls(int playerIndex, PlayerController controller)
    {
        GUILayout.Label($"=== PLAYER {playerIndex + 1} ===", headerStyle);
        GUILayout.Space(5);
        
        if (controller == null)
        {
            GUILayout.Label("Controller not assigned", labelStyle);
            return;
        }
        
        var bindings = controller.Bindings;
        if (bindings == null)
        {
            GUILayout.Label("No bindings found", labelStyle);
            return;
        }
        
        DrawBindingRow(playerIndex, "Cursor Up", "cursorUp", bindings.cursorUp);
        DrawBindingRow(playerIndex, "Cursor Down", "cursorDown", bindings.cursorDown);
        DrawBindingRow(playerIndex, "Cursor Left", "cursorLeft", bindings.cursorLeft);
        DrawBindingRow(playerIndex, "Cursor Right", "cursorRight", bindings.cursorRight);
        
        GUILayout.Space(5);
        
        DrawBindingRow(playerIndex, "Send to Sanctuary", "sendToSanctuary", bindings.sendToSanctuary);
        DrawBindingRow(playerIndex, "Send from Sanctuary", "sendFromSanctuary", bindings.sendFromSanctuary);
        DrawBindingRow(playerIndex, "Upgrade Room", "upgradeRoom", bindings.upgradeRoom);
        
        GUILayout.Space(5);
        
        DrawBindingRow(playerIndex, "Use Mask 1", "useMask1", bindings.useMask1);
        DrawBindingRow(playerIndex, "Use Mask 2", "useMask2", bindings.useMask2);
        DrawBindingRow(playerIndex, "Use Mask 3", "useMask3", bindings.useMask3);
        DrawBindingRow(playerIndex, "Use Mask 4", "useMask4", bindings.useMask4);
        
        GUILayout.Space(5);
        
        DrawBindingRow(playerIndex, "Confirm Target", "confirmTarget", bindings.confirmTarget);
        DrawBindingRow(playerIndex, "Cancel Target", "cancelTarget", bindings.cancelTarget);
    }
    
    private void DrawBindingRow(int playerIndex, string actionName, string fieldName, KeyCode currentKey)
    {
        GUILayout.BeginHorizontal();
        
        GUILayout.Label(actionName, labelStyle, GUILayout.Width(150));
        
        string keyDisplay = currentKey.ToString();
        if (GUILayout.Button(keyDisplay, bindingStyle, GUILayout.Width(100)))
        {
            StartRebinding(playerIndex, fieldName, actionName);
        }
        
        GUILayout.EndHorizontal();
    }
    
    private void StartRebinding(int playerIndex, string fieldName, string actionName)
    {
        isRebinding = true;
        rebindingPlayer = playerIndex;
        rebindingAction = actionName;
        
        // Store the field name for later
        PlayerPrefs.SetString("RebindingField", fieldName);
    }
    
    private void ApplyRebinding(KeyCode newKey)
    {
        string fieldName = PlayerPrefs.GetString("RebindingField", "");
        
        var controller = rebindingPlayer == 0 ? player1 : player2;
        if (controller == null) 
        {
            CancelRebinding();
            return;
        }
        
        var bindings = controller.Bindings;
        
        // Use reflection to set the field
        var field = typeof(PlayerInputBindings).GetField(fieldName);
        if (field != null)
        {
            field.SetValue(bindings, newKey);
            Debug.Log($"Rebound {fieldName} to {newKey} for Player {rebindingPlayer + 1}");
        }
        
        CancelRebinding();
    }
    
    private void CancelRebinding()
    {
        isRebinding = false;
        rebindingPlayer = -1;
        rebindingAction = null;
    }
    
    private void ResetToDefaults(int playerIndex)
    {
        var controller = playerIndex == 0 ? player1 : player2;
        if (controller == null) return;
        
        var defaults = playerIndex == 0 
            ? PlayerInputBindings.CreatePlayer1Defaults() 
            : PlayerInputBindings.CreatePlayer2Defaults();
        
        // Copy defaults to current bindings
        var bindings = controller.Bindings;
        var fields = typeof(PlayerInputBindings).GetFields();
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(KeyCode))
            {
                field.SetValue(bindings, field.GetValue(defaults));
            }
        }
        
        Debug.Log($"Reset Player {playerIndex + 1} controls to defaults");
    }
    
    public void SetControllers(PlayerController p1, PlayerController p2)
    {
        player1 = p1;
        player2 = p2;
    }
}
