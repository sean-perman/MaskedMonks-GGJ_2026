using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// In-game menu for viewing and rebinding controls for both players.
/// Opened externally (e.g. by PauseMenu); Escape closes it.
/// </summary>
public class ControlsMenu : MonoBehaviour
{
    [Header("Settings")]
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
    private string rebindingFieldName = null;
    private string rebindingDeviceInfo = null;

    private Vector2 scrollPosition;

    private float previousTimeScale = 1f;

    public bool IsOpen => isOpen;

    public void Show() => SetOpen(true);
    public void Hide() => SetOpen(false);

    public void SetOpen(bool open)
    {
        if (open == isOpen) return;
        isOpen = open;
        if (isOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            CancelRebinding();
            Time.timeScale = previousTimeScale;
        }
    }

    private void Update()
    {
        // Escape closes the menu (open is initiated externally - typically by PauseMenu).
        if (isOpen && !isRebinding && Input.GetKeyDown(KeyCode.Escape))
        {
            SetOpen(false);
            return;
        }

        // Handle rebinding
        if (isRebinding)
        {
            // Check gamepads via new Input System first and apply as gamepad bindings
            var gamepads = Gamepad.all;
            for (int i = 0; i < gamepads.Count; i++)
            {
                var gp = gamepads[i];
                if (gp == null) continue;

                // Face buttons: map to indices 0(A)/1(B)/2(X)/3(Y)
                if (gp.buttonSouth.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 0);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }
                if (gp.buttonEast.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 1);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }
                if (gp.buttonWest.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 2);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }
                if (gp.buttonNorth.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 3);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }

                // Shoulders
                if (gp.leftShoulder.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 4);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }
                if (gp.rightShoulder.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 5);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }

                // D-pad mapping to indices 9-12 (we'll store these as 9..12)
                if (gp.dpad.up.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 9);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }
                if (gp.dpad.down.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 10);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }
                if (gp.dpad.left.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 11);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }
                if (gp.dpad.right.wasPressedThisFrame)
                {
                    ApplyRebindingGamepad(i, 12);
                    rebindingDeviceInfo = $"Gamepad {i + 1}";
                    return;
                }
            }

            // Fall back to legacy Input key iteration for keyboard keys (keeps existing rebinding behavior)
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
            
            // Gamepad preset buttons
            GUILayout.BeginHorizontal();
            GUILayout.Label("Presets:", labelStyle, GUILayout.Width(100));
            
            if (GUILayout.Button("P1 Gamepad", GUILayout.Height(30)))
            {
                ApplyGamepadPreset(0);
            }
            
            if (GUILayout.Button("P2 Gamepad", GUILayout.Height(30)))
            {
                ApplyGamepadPreset(1);
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Reset/Close buttons
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
                SetOpen(false);
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

        // Also show gamepad mapping if present
        string gamepadDisplay = "";
        var controller = playerIndex == 0 ? player1 : player2;
        if (controller != null && controller.Bindings != null)
        {
            // Map fieldName to gamepad field name
            string gpField = GetGamepadFieldName(fieldName);
            if (!string.IsNullOrEmpty(gpField))
            {
                var gpFieldInfo = typeof(PlayerInputBindings).GetField(gpField);
                if (gpFieldInfo != null)
                {
                    var val = gpFieldInfo.GetValue(controller.Bindings);
                    if (val is int gi && gi >= 0)
                    {
                        gamepadDisplay = $"G{(gi==9||gi==10||gi==11||gi==12?"Dpad":"Btn")}{gi}";
                    }
                }
            }
        }

        string display = string.IsNullOrEmpty(gamepadDisplay) ? keyDisplay : $"{keyDisplay} / {gamepadDisplay}";
        if (GUILayout.Button(display, bindingStyle, GUILayout.Width(160)))
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
        rebindingFieldName = fieldName;
        rebindingDeviceInfo = null;
        
        // Store the field name for later (legacy consumer)
        PlayerPrefs.SetString("RebindingField", fieldName);
    }
    
    private void ApplyRebinding(KeyCode newKey)
    {
        string fieldName = rebindingFieldName ?? PlayerPrefs.GetString("RebindingField", "");

        var controller = rebindingPlayer == 0 ? player1 : player2;
        if (controller == null)
        {
            CancelRebinding();
            return;
        }

        var bindings = controller.Bindings;

        // Use reflection to set the keyboard KeyCode field
        var field = typeof(PlayerInputBindings).GetField(fieldName);
        if (field != null && field.FieldType == typeof(KeyCode))
        {
            field.SetValue(bindings, newKey);
            Debug.Log($"Rebound {fieldName} to {newKey} for Player {rebindingPlayer + 1}");
        }

        CancelRebinding();
    }

    private void ApplyRebindingGamepad(int deviceIndex, int buttonIndex)
    {
        string fieldName = rebindingFieldName ?? PlayerPrefs.GetString("RebindingField", "");

        var controller = rebindingPlayer == 0 ? player1 : player2;
        if (controller == null) { CancelRebinding(); return; }

        var bindings = controller.Bindings;

        // Map the keyboard field name to the corresponding gamepad field name
        string gpField = GetGamepadFieldName(fieldName);
        if (string.IsNullOrEmpty(gpField))
        {
            // No gamepad field for this action; try to set confirm/cancel if appropriate
            Debug.LogWarning($"No gamepad field mapping for {fieldName}");
            CancelRebinding();
            return;
        }

        var gpFieldInfo = typeof(PlayerInputBindings).GetField(gpField);
        if (gpFieldInfo != null && gpFieldInfo.FieldType == typeof(int))
        {
            gpFieldInfo.SetValue(bindings, buttonIndex);
            Debug.Log($"Rebound {gpField} to button {buttonIndex} (device {deviceIndex + 1}) for Player {rebindingPlayer + 1}");
        }

        CancelRebinding();
    }

    private string GetGamepadFieldName(string keyFieldName)
    {
        switch (keyFieldName)
        {
            case "cursorUp": return "gamepadCursorUp";
            case "cursorDown": return "gamepadCursorDown";
            case "cursorLeft": return "gamepadCursorLeft";
            case "cursorRight": return "gamepadCursorRight";
            case "sendToSanctuary": return "gamepadSendToSanctuary";
            case "sendFromSanctuary": return "gamepadSendFromSanctuary";
            case "upgradeRoom": return "gamepadUpgradeRoom";
            case "useMask1": return "gamepadUseMask1";
            case "useMask2": return "gamepadUseMask2";
            case "useMask3": return "gamepadUseMask3";
            case "useMask4": return "gamepadUseMask4";
            case "confirmTarget": return "gamepadConfirmTarget";
            case "cancelTarget": return "gamepadCancelTarget";
            default: return null;
        }
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
            if (field.FieldType == typeof(KeyCode) || field.FieldType == typeof(int) || field.FieldType == typeof(bool))
            {
                field.SetValue(bindings, field.GetValue(defaults));
            }
        }
        
        Debug.Log($"Reset Player {playerIndex + 1} controls to defaults");
    }
    
    private void ApplyGamepadPreset(int playerIndex)
    {
        var controller = playerIndex == 0 ? player1 : player2;
        if (controller == null) return;
        
        // Apply gamepad defaults and switch to gamepad mode
        controller.ApplyGamepadDefaults();
        controller.SetUseGamepad(true);
        
        Debug.Log($"Applied gamepad preset to Player {playerIndex + 1}");
    }
    
    public void SetControllers(PlayerController p1, PlayerController p2)
    {
        player1 = p1;
        player2 = p2;
    }
}
