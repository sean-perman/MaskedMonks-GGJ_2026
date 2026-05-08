using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Title screen controller. Shown in the MainMenu scene; loads the gameplay
/// scene when "Start Game" is pressed. Tilde opens the config editor;
/// the Controls button opens the input-binding editor.
/// </summary>
[RequireComponent(typeof(ConfigEditorMenu))]
[RequireComponent(typeof(MainMenuControlsScreen))]
public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Name of the gameplay scene to load (must be in Build Settings).")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Display")]
    [SerializeField] private string title = "MASKED MONKS";
    [SerializeField] private string subtitle = "DU Arcade Edition";

    [Header("Config Editor Hotkey")]
    [Tooltip("Key that opens the config editor on the main menu. Default is tilde / backtick (`).")]
    [SerializeField] private KeyCode configHotkey = KeyCode.BackQuote;
    [Tooltip("If true, the hotkey requires Ctrl+Shift to be held as well.")]
    [SerializeField] private bool requireCtrlShift = false;
    [Tooltip("If true, shows a small hint at the bottom of the screen indicating the hotkey.")]
    [SerializeField] private bool showHotkeyHint = true;

    private ConfigEditorMenu configEditor;
    private MainMenuControlsScreen controlsScreen;

    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle hintStyle;
    private bool stylesInit;

    private void Awake()
    {
        // Get-or-add so older saved scenes that predate the [RequireComponent]
        // attributes still have the helper components attached at runtime.
        configEditor = GetComponent<ConfigEditorMenu>();
        if (configEditor == null) configEditor = gameObject.AddComponent<ConfigEditorMenu>();

        controlsScreen = GetComponent<MainMenuControlsScreen>();
        if (controlsScreen == null) controlsScreen = gameObject.AddComponent<MainMenuControlsScreen>();

        // Reset timeScale in case we returned here from a paused state.
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Don't intercept the config hotkey while the controls screen is open
        // (it has its own input flow for rebinding).
        if (controlsScreen != null && controlsScreen.IsOpen) return;

        bool comboHeld = !requireCtrlShift ||
            ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
          && (Input.GetKey(KeyCode.LeftShift)   || Input.GetKey(KeyCode.RightShift)));

        if (comboHeld && Input.GetKeyDown(configHotkey))
        {
            configEditor.Toggle();
        }
    }

    private void InitStyles()
    {
        if (stylesInit) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 64,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = Color.white;

        subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };
        subtitleStyle.normal.textColor = new Color(1, 1, 1, 0.7f);

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter
        };

        hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleRight
        };
        hintStyle.normal.textColor = new Color(1, 1, 1, 0.35f);

        stylesInit = true;
    }

    private void OnGUI()
    {
        // Let overlay components draw on top when open.
        if (configEditor != null && configEditor.IsOpen) return;
        if (controlsScreen != null && controlsScreen.IsOpen) return;

        InitStyles();

        // Solid backdrop so the title screen reads even without a scene background.
        GUI.color = new Color(0.05f, 0.05f, 0.1f, 1f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title block
        GUI.Label(new Rect(0, Screen.height * 0.18f, Screen.width, 90f), title, titleStyle);
        GUI.Label(new Rect(0, Screen.height * 0.18f + 80f, Screen.width, 30f), subtitle, subtitleStyle);

        // Buttons
        const float btnW = 320f;
        const float btnH = 60f;
        const float gap = 16f;

        float x = (Screen.width - btnW) / 2f;
        float y = Screen.height * 0.45f;

        if (GUI.Button(new Rect(x, y, btnW, btnH), "Start Game", buttonStyle))
        {
            StartGame();
        }
        y += btnH + gap;

        if (GUI.Button(new Rect(x, y, btnW, btnH), "Controls", buttonStyle))
        {
            controlsScreen.SetVisible(true);
        }
        y += btnH + gap;

        if (GUI.Button(new Rect(x, y, btnW, btnH), "Quit", buttonStyle))
        {
            Quit();
        }

        // Saved-config indicator
        if (GameConfigPersistence.HasSavedFile)
        {
            var savedStyle = new GUIStyle(hintStyle) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, y + btnH + 20f, Screen.width, 20f), "Custom config loaded.", savedStyle);
        }

        if (showHotkeyHint)
        {
            string keyLabel = DisplayKey(configHotkey);
            string hint = requireCtrlShift
                ? $"Press Ctrl+Shift+{keyLabel} for config editor"
                : $"Press {keyLabel} for config editor";
            GUI.Label(new Rect(0, Screen.height - 24f, Screen.width - 12f, 20f), hint, hintStyle);
        }
    }

    private static string DisplayKey(KeyCode key)
    {
        return key switch
        {
            KeyCode.BackQuote => "~",
            KeyCode.Tilde => "~",
            _ => key.ToString()
        };
    }

    private void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
