using UnityEngine;

/// <summary>
/// Console-style debug log that displays recent game events on screen.
/// Captures Debug.Log messages and displays them in an overlay.
/// </summary>
public class DebugConsole : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool showConsole = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // ` key
    [SerializeField] private int maxMessages = 20;
    [SerializeField] private float messageLifetime = 10f;
    
    [Header("Layout")]
    [SerializeField] private float consoleWidth = 500f;
    [SerializeField] private float consoleHeight = 300f;
    [SerializeField] private float padding = 10f;
    
    private struct LogMessage
    {
        public string text;
        public LogType type;
        public float timestamp;
    }
    
    private LogMessage[] messages;
    private int messageIndex = 0;
    private int messageCount = 0;
    private Vector2 scrollPosition;
    
    private GUIStyle logStyle;
    private GUIStyle warningStyle;
    private GUIStyle errorStyle;
    private GUIStyle headerStyle;
    private bool stylesInit = false;
    
    private void Awake()
    {
        messages = new LogMessage[maxMessages];
        Application.logMessageReceived += HandleLog;
    }
    
    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showConsole = !showConsole;
        }
    }
    
    private void HandleLog(string message, string stackTrace, LogType type)
    {
        // Only capture certain types
        if (type == LogType.Log || type == LogType.Warning || type == LogType.Error)
        {
            messages[messageIndex] = new LogMessage
            {
                text = message,
                type = type,
                timestamp = Time.time
            };
            
            messageIndex = (messageIndex + 1) % maxMessages;
            if (messageCount < maxMessages) messageCount++;
            
            // Auto-scroll to bottom
            scrollPosition.y = float.MaxValue;
        }
    }
    
    private void InitStyles()
    {
        if (stylesInit) return;
        
        logStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
        logStyle.normal.textColor = Color.white;
        logStyle.wordWrap = true;
        
        warningStyle = new GUIStyle(logStyle);
        warningStyle.normal.textColor = Color.yellow;
        
        errorStyle = new GUIStyle(logStyle);
        errorStyle.normal.textColor = Color.red;
        
        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headerStyle.normal.textColor = Color.white;
        
        stylesInit = true;
    }
    
    private void OnGUI()
    {
        if (!showConsole) return;
        
        InitStyles();
        
        // Position at bottom-left
        Rect consoleRect = new Rect(
            padding,
            Screen.height - consoleHeight - padding,
            consoleWidth,
            consoleHeight
        );
        
        // Background
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.Box(consoleRect, "");
        GUI.color = Color.white;
        
        GUILayout.BeginArea(new Rect(consoleRect.x + 5, consoleRect.y + 5, consoleRect.width - 10, consoleRect.height - 10));
        
        // Header
        GUILayout.BeginHorizontal();
        GUILayout.Label("DEBUG CONSOLE", headerStyle);
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            messageCount = 0;
            messageIndex = 0;
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Messages
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < messageCount; i++)
        {
            int idx = (messageIndex - messageCount + i + maxMessages) % maxMessages;
            var msg = messages[idx];
            
            // Skip expired messages
            if (Time.time - msg.timestamp > messageLifetime) continue;
            
            GUIStyle style = msg.type switch
            {
                LogType.Warning => warningStyle,
                LogType.Error => errorStyle,
                _ => logStyle
            };
            
            string prefix = msg.type switch
            {
                LogType.Warning => "[!] ",
                LogType.Error => "[X] ",
                _ => ""
            };
            
            float age = Time.time - msg.timestamp;
            float alpha = Mathf.Clamp01(1f - (age / messageLifetime) * 0.5f);
            
            GUI.color = new Color(1, 1, 1, alpha);
            GUILayout.Label($"{prefix}{msg.text}", style);
        }
        
        GUI.color = Color.white;
        GUILayout.EndScrollView();
        
        GUILayout.EndArea();
        
        // Toggle hint
        GUI.Label(new Rect(padding, Screen.height - 20, 200, 16), $"Press {toggleKey} to toggle console");
    }
}
