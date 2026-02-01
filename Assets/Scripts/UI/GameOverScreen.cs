using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Over screen displayed when a cult wins or loses.
/// Shows winner, loser, and reason, with options to restart or quit.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.85f);
    [SerializeField] private Color winnerColor = new Color(0.3f, 1f, 0.4f);
    [SerializeField] private Color loserColor = new Color(1f, 0.3f, 0.3f);
    
    // State
    private bool isShowing = false;
    private float showTime = 0f;
    private Cult winner;
    private Cult loser;
    private string reason;
    
    // Styles
    private GUIStyle titleStyle;
    private GUIStyle winnerStyle;
    private GUIStyle loserStyle;
    private GUIStyle reasonStyle;
    private GUIStyle buttonStyle;
    private GUIStyle overlayStyle;
    private Texture2D overlayTexture;
    private bool stylesInit = false;
    
    private void OnEnable()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameEnded += OnGameEnded;
        }
    }
    
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameEnded -= OnGameEnded;
        }
    }
    
    private void Start()
    {
        // Try to subscribe if GameManager wasn't ready in OnEnable
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameEnded -= OnGameEnded; // Avoid double subscribe
            GameManager.Instance.OnGameEnded += OnGameEnded;
        }
    }
    
    private void OnGameEnded()
    {
        // Get winner/loser from GameManager - we need to listen to the specific events
    }
    
    /// <summary>
    /// Show the game over screen with the specified results.
    /// </summary>
    public void Show(Cult winner, Cult loser, string reason)
    {
        this.winner = winner;
        this.loser = loser;
        this.reason = reason;
        this.isShowing = true;
        this.showTime = Time.unscaledTime;
        
        // Pause the game
        Time.timeScale = 0f;
        
        Debug.Log($"GameOverScreen: Showing - Winner: {winner?.name}, Loser: {loser?.name}, Reason: {reason}");
    }
    
    /// <summary>
    /// Hide the game over screen.
    /// </summary>
    public void Hide()
    {
        isShowing = false;
        Time.timeScale = 1f;
    }
    
    private void InitStyles()
    {
        if (stylesInit) return;
        
        // Create overlay texture
        overlayTexture = new Texture2D(1, 1);
        overlayTexture.SetPixel(0, 0, Color.white);
        overlayTexture.Apply();
        
        overlayStyle = new GUIStyle();
        overlayStyle.normal.background = overlayTexture;
        
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = Color.white;
        
        winnerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        winnerStyle.normal.textColor = winnerColor;
        
        loserStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter
        };
        loserStyle.normal.textColor = loserColor;
        
        reasonStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        reasonStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        
        stylesInit = true;
    }
    
    private void OnGUI()
    {
        if (!isShowing) return;
        
        InitStyles();
        
        // Calculate fade alpha
        float elapsed = Time.unscaledTime - showTime;
        float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
        
        // Draw overlay
        Color fadeColor = overlayColor;
        fadeColor.a *= alpha;
        GUI.color = fadeColor;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);
        GUI.color = Color.white;
        
        // Content fades in
        Color contentColor = Color.white;
        contentColor.a = alpha;
        GUI.color = contentColor;
        
        // Calculate center positions
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        float boxWidth = 500f;
        float boxHeight = 400f;
        float boxX = centerX - boxWidth / 2f;
        float boxY = centerY - boxHeight / 2f;
        
        // Draw content
        GUILayout.BeginArea(new Rect(boxX, boxY, boxWidth, boxHeight));
        GUILayout.FlexibleSpace();
        
        // Title
        GUILayout.Label("GAME OVER", titleStyle);
        GUILayout.Space(30);
        
        // Winner
        string winnerName = GetCultDisplayName(winner);
        GUI.color = new Color(winnerColor.r, winnerColor.g, winnerColor.b, alpha);
        GUILayout.Label($"🏆 {winnerName} WINS! 🏆", winnerStyle);
        GUI.color = contentColor;
        
        GUILayout.Space(20);
        
        // Loser
        string loserName = GetCultDisplayName(loser);
        GUI.color = new Color(loserColor.r, loserColor.g, loserColor.b, alpha);
        GUILayout.Label($"{loserName} has fallen", loserStyle);
        GUI.color = contentColor;
        
        GUILayout.Space(15);
        
        // Reason
        if (!string.IsNullOrEmpty(reason))
        {
            GUILayout.Label(reason, reasonStyle);
        }
        
        GUILayout.Space(40);
        
        // Buttons
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Play Again", buttonStyle, GUILayout.Width(150), GUILayout.Height(50)))
        {
            RestartGame();
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Quit", buttonStyle, GUILayout.Width(150), GUILayout.Height(50)))
        {
            QuitGame();
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
        
        GUI.color = Color.white;
    }
    
    private string GetCultDisplayName(Cult cult)
    {
        if (cult == null) return "Unknown";
        
        // Try to determine player number from GameManager
        if (GameManager.Instance != null)
        {
            if (cult == GameManager.Instance.Cult1) return "Player 1";
            if (cult == GameManager.Instance.Cult2) return "Player 2";
        }
        
        return cult.name;
    }
    
    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void QuitGame()
    {
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
