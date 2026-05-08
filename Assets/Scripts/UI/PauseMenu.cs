using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// In-game pause menu, opened by Escape. Pauses the game and shows
/// Resume / Controls / Main Menu / Quit buttons. Coordinates with
/// ControlsMenu so Escape doesn't double-fire when a child overlay is open.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Scene Flow")]
    [Tooltip("Name of the main menu scene to return to.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("References")]
    [SerializeField] private ControlsMenu controlsMenu;

    private bool isOpen;
    private float previousTimeScale = 1f;

    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private bool stylesInit;

    public bool IsOpen => isOpen;

    public void SetReferences(ControlsMenu controls)
    {
        controlsMenu = controls;
    }

    private void Update()
    {
        // Don't double-handle Escape while the controls overlay is up - it owns
        // its own close-on-Escape so the user can step back to this menu.
        if (controlsMenu != null && controlsMenu.IsOpen) return;

        // Don't open the pause menu when the game is over (the GameOverScreen
        // is handling it) or when the game hasn't started yet.
        if (GameManager.Instance != null && !GameManager.Instance.IsGameRunning && !isOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

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
            Time.timeScale = previousTimeScale;
        }
    }

    private void InitStyles()
    {
        if (stylesInit) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        stylesInit = true;
    }

    private void OnGUI()
    {
        if (!isOpen) return;

        // Hide ourselves while the controls overlay is on top so the two don't
        // visually stack.
        if (controlsMenu != null && controlsMenu.IsOpen) return;

        InitStyles();

        // Dim background.
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(0, Screen.height * 0.18f, Screen.width, 80f), "PAUSED", titleStyle);

        const float btnW = 320f;
        const float btnH = 60f;
        const float gap = 16f;

        float x = (Screen.width - btnW) / 2f;
        float y = Screen.height * 0.38f;

        if (GUI.Button(new Rect(x, y, btnW, btnH), "Resume", buttonStyle))
        {
            SetVisible(false);
        }
        y += btnH + gap;

        if (GUI.Button(new Rect(x, y, btnW, btnH), "Controls", buttonStyle))
        {
            if (controlsMenu != null)
            {
                controlsMenu.Show();
            }
            else
            {
                Debug.LogWarning("PauseMenu: no ControlsMenu reference assigned.");
            }
        }
        y += btnH + gap;

        if (GUI.Button(new Rect(x, y, btnW, btnH), "Main Menu", buttonStyle))
        {
            ReturnToMainMenu();
        }
        y += btnH + gap;

        if (GUI.Button(new Rect(x, y, btnW, btnH), "Quit", buttonStyle))
        {
            Quit();
        }
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private static void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
