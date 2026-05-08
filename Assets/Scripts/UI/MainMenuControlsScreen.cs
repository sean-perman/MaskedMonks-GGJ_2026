using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main-menu controls editor. Edits the persisted PlayerInputBindings for both
/// players directly (no PlayerController needed). Save writes JSON via
/// BindingsPersistence, which PlayerController reads on Initialize.
///
/// Modeled on the in-game ControlsMenu but decoupled from PlayerController so it
/// can run on the title screen.
/// </summary>
public class MainMenuControlsScreen : MonoBehaviour
{
    private bool isOpen;
    private float previousTimeScale = 1f;

    private bool isRebinding;
    private int rebindingPlayer = -1;
    private string rebindingActionLabel;
    private string rebindingFieldName;

    private Vector2 scrollPosition;
    private string statusMessage = "";
    private float statusUntilTime;

    private GUIStyle headerStyle;
    private GUIStyle sectionStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle bindingStyle;
    private GUIStyle pathStyle;
    private bool stylesInit;

    public bool IsOpen => isOpen;

    public void Toggle() => SetVisible(!isOpen);

    public void SetVisible(bool visible)
    {
        if (visible == isOpen) return;
        isOpen = visible;

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

    private static BindingsPersistence.BindingsData Data => BindingsPersistence.GetOrLoadCache();
    private static PlayerInputBindings Bindings(int playerIndex) =>
        playerIndex == 0 ? Data.player1 : Data.player2;

    private void Update()
    {
        if (!isOpen) return;

        if (isRebinding)
        {
            // Cancel rebinding with escape (don't close the whole screen).
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelRebinding();
                return;
            }

            // Try gamepad face/shoulder/dpad first.
            var gamepads = Gamepad.all;
            for (int i = 0; i < gamepads.Count; i++)
            {
                var gp = gamepads[i];
                if (gp == null) continue;

                if (gp.buttonSouth.wasPressedThisFrame) { ApplyRebindingGamepad(0); return; }
                if (gp.buttonEast.wasPressedThisFrame)  { ApplyRebindingGamepad(1); return; }
                if (gp.buttonWest.wasPressedThisFrame)  { ApplyRebindingGamepad(2); return; }
                if (gp.buttonNorth.wasPressedThisFrame) { ApplyRebindingGamepad(3); return; }
                if (gp.leftShoulder.wasPressedThisFrame)  { ApplyRebindingGamepad(4); return; }
                if (gp.rightShoulder.wasPressedThisFrame) { ApplyRebindingGamepad(5); return; }
                if (gp.dpad.up.wasPressedThisFrame)    { ApplyRebindingGamepad(9); return; }
                if (gp.dpad.down.wasPressedThisFrame)  { ApplyRebindingGamepad(10); return; }
                if (gp.dpad.left.wasPressedThisFrame)  { ApplyRebindingGamepad(11); return; }
                if (gp.dpad.right.wasPressedThisFrame) { ApplyRebindingGamepad(12); return; }
            }

            // Fall back to legacy keyboard.
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key) && key != KeyCode.Escape && key != KeyCode.Mouse0)
                {
                    ApplyRebindingKey(key);
                    return;
                }
            }
            return;
        }

        // Close on escape when not actively rebinding.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetVisible(false);
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

        sectionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        sectionStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);

        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        labelStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 13 };

        bindingStyle = new GUIStyle(GUI.skin.button) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
        bindingStyle.normal.textColor = Color.cyan;

        pathStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
        pathStyle.normal.textColor = new Color(1, 1, 1, 0.55f);

        stylesInit = true;
    }

    private void OnGUI()
    {
        if (!isOpen) return;

        InitStyles();

        // Dim background.
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float w = Mathf.Min(900f, Screen.width - 80f);
        float h = Mathf.Min(700f, Screen.height - 80f);
        Rect window = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
        GUI.Box(window, GUIContent.none);

        GUILayout.BeginArea(new Rect(window.x + 16, window.y + 16, window.width - 32, window.height - 32));

        GUILayout.Label("CONTROLS", headerStyle);
        GUILayout.Label($"Save file: {BindingsPersistence.FilePath}", pathStyle);
        if (Time.unscaledTime < statusUntilTime)
            GUILayout.Label(statusMessage, labelStyle);
        else
            GUILayout.Space(15);

        GUILayout.Space(4);

        if (isRebinding)
        {
            GUILayout.FlexibleSpace();
            var rebindStyle = new GUIStyle(headerStyle) { fontSize = 18 };
            GUILayout.Label($"Press a key or gamepad button for", rebindStyle);
            GUILayout.Label($"\"{rebindingActionLabel}\" (Player {rebindingPlayer + 1})", rebindStyle);
            GUILayout.Space(12);
            GUILayout.Label("Press Escape to cancel", labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
            return;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width((window.width - 64) / 2f));
        DrawPlayerColumn(0);
        GUILayout.EndVertical();

        GUILayout.Space(20);

        GUILayout.BeginVertical(GUILayout.Width((window.width - 64) / 2f));
        DrawPlayerColumn(1);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", buttonStyle, GUILayout.Height(32)))
        {
            if (BindingsPersistence.Save()) SetStatus("Saved.");
            else SetStatus("Save failed - see console.");
        }
        if (GUILayout.Button("Reload from File", buttonStyle, GUILayout.Height(32)))
        {
            if (BindingsPersistence.Load()) SetStatus("Reloaded from file.");
            else SetStatus(BindingsPersistence.HasSavedFile ? "Reload failed - see console." : "No saved file yet.");
        }
        if (GUILayout.Button("Reset Both to Defaults", buttonStyle, GUILayout.Height(32)))
        {
            BindingsPersistence.ResetToDefaults();
            SetStatus("Reset to defaults (not saved).");
        }
        if (GUILayout.Button("Close", buttonStyle, GUILayout.Height(32)))
        {
            SetVisible(false);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void DrawPlayerColumn(int playerIndex)
    {
        GUILayout.Label($"PLAYER {playerIndex + 1}", sectionStyle);
        GUILayout.Space(6);

        var b = Bindings(playerIndex);

        GUILayout.Label($"Mode: {(b.useGamepad ? "Gamepad" : "Keyboard")}", labelStyle);
        GUILayout.Space(4);

        DrawBindingRow(playerIndex, "Cursor Up", "cursorUp", b.cursorUp);
        DrawBindingRow(playerIndex, "Cursor Down", "cursorDown", b.cursorDown);
        DrawBindingRow(playerIndex, "Cursor Left", "cursorLeft", b.cursorLeft);
        DrawBindingRow(playerIndex, "Cursor Right", "cursorRight", b.cursorRight);

        GUILayout.Space(6);
        DrawBindingRow(playerIndex, "Send to Sanctuary", "sendToSanctuary", b.sendToSanctuary);
        DrawBindingRow(playerIndex, "Send from Sanctuary", "sendFromSanctuary", b.sendFromSanctuary);
        DrawBindingRow(playerIndex, "Upgrade Room", "upgradeRoom", b.upgradeRoom);

        GUILayout.Space(6);
        DrawBindingRow(playerIndex, "Use Mask 1", "useMask1", b.useMask1);
        DrawBindingRow(playerIndex, "Use Mask 2", "useMask2", b.useMask2);
        DrawBindingRow(playerIndex, "Use Mask 3", "useMask3", b.useMask3);
        DrawBindingRow(playerIndex, "Use Mask 4", "useMask4", b.useMask4);

        GUILayout.Space(6);
        DrawBindingRow(playerIndex, "Confirm Target", "confirmTarget", b.confirmTarget);
        DrawBindingRow(playerIndex, "Cancel Target", "cancelTarget", b.cancelTarget);

        GUILayout.Space(8);

        if (GUILayout.Button("Apply Gamepad Preset", buttonStyle, GUILayout.Height(28)))
        {
            ApplyGamepadPreset(playerIndex);
        }
        if (GUILayout.Button("Reset to Defaults", buttonStyle, GUILayout.Height(28)))
        {
            ResetPlayer(playerIndex);
        }
    }

    private void DrawBindingRow(int playerIndex, string actionLabel, string fieldName, KeyCode currentKey)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(actionLabel, labelStyle, GUILayout.Width(150));

        string keyDisplay = currentKey.ToString();
        string gamepadDisplay = "";

        var b = Bindings(playerIndex);
        string gpField = GetGamepadFieldName(fieldName);
        if (!string.IsNullOrEmpty(gpField))
        {
            var gpFieldInfo = typeof(PlayerInputBindings).GetField(gpField);
            if (gpFieldInfo != null && gpFieldInfo.GetValue(b) is int gi && gi >= 0)
            {
                gamepadDisplay = $"G{(gi == 9 || gi == 10 || gi == 11 || gi == 12 ? "Dpad" : "Btn")}{gi}";
            }
        }

        string display = string.IsNullOrEmpty(gamepadDisplay) ? keyDisplay : $"{keyDisplay} / {gamepadDisplay}";
        if (GUILayout.Button(display, bindingStyle, GUILayout.Width(150)))
        {
            StartRebinding(playerIndex, fieldName, actionLabel);
        }

        GUILayout.EndHorizontal();
    }

    private void StartRebinding(int playerIndex, string fieldName, string actionLabel)
    {
        isRebinding = true;
        rebindingPlayer = playerIndex;
        rebindingActionLabel = actionLabel;
        rebindingFieldName = fieldName;
    }

    private void ApplyRebindingKey(KeyCode key)
    {
        var b = Bindings(rebindingPlayer);
        var field = typeof(PlayerInputBindings).GetField(rebindingFieldName);
        if (field != null && field.FieldType == typeof(KeyCode))
        {
            field.SetValue(b, key);
            SetStatus($"P{rebindingPlayer + 1} {rebindingActionLabel} -> {key}");
        }
        CancelRebinding();
    }

    private void ApplyRebindingGamepad(int buttonIndex)
    {
        var b = Bindings(rebindingPlayer);
        string gpField = GetGamepadFieldName(rebindingFieldName);
        if (!string.IsNullOrEmpty(gpField))
        {
            var info = typeof(PlayerInputBindings).GetField(gpField);
            if (info != null && info.FieldType == typeof(int))
            {
                info.SetValue(b, buttonIndex);
                SetStatus($"P{rebindingPlayer + 1} {rebindingActionLabel} -> Gamepad btn {buttonIndex}");
            }
        }
        CancelRebinding();
    }

    private static string GetGamepadFieldName(string keyFieldName)
    {
        return keyFieldName switch
        {
            "cursorUp" => "gamepadCursorUp",
            "cursorDown" => "gamepadCursorDown",
            "cursorLeft" => "gamepadCursorLeft",
            "cursorRight" => "gamepadCursorRight",
            "sendToSanctuary" => "gamepadSendToSanctuary",
            "sendFromSanctuary" => "gamepadSendFromSanctuary",
            "upgradeRoom" => "gamepadUpgradeRoom",
            "useMask1" => "gamepadUseMask1",
            "useMask2" => "gamepadUseMask2",
            "useMask3" => "gamepadUseMask3",
            "useMask4" => "gamepadUseMask4",
            "confirmTarget" => "gamepadConfirmTarget",
            "cancelTarget" => "gamepadCancelTarget",
            _ => null
        };
    }

    private void CancelRebinding()
    {
        isRebinding = false;
        rebindingPlayer = -1;
        rebindingActionLabel = null;
        rebindingFieldName = null;
    }

    private void ApplyGamepadPreset(int playerIndex)
    {
        var data = Data;
        var preset = PlayerInputBindings.CreateGamepadDefaults(playerIndex);
        preset.ApplyGamepadKeyMappings(playerIndex);
        if (playerIndex == 0) data.player1 = preset;
        else data.player2 = preset;
        SetStatus($"P{playerIndex + 1} gamepad preset applied (not saved).");
    }

    private void ResetPlayer(int playerIndex)
    {
        var data = Data;
        if (playerIndex == 0) data.player1 = PlayerInputBindings.CreatePlayer1Defaults();
        else data.player2 = PlayerInputBindings.CreatePlayer2Defaults();
        SetStatus($"P{playerIndex + 1} reset to defaults (not saved).");
    }

    private void SetStatus(string msg)
    {
        statusMessage = msg;
        statusUntilTime = Time.unscaledTime + 4f;
    }
}
